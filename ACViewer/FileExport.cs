using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

/*using Aspose.ThreeD;
using Aspose.ThreeD.Animation;
using Aspose.ThreeD.Entities;*/

using Assimp;

using UkooLabs.FbxSharpie;

using ACE.DatLoader;
using ACE.DatLoader.Entity;
using ACE.DatLoader.FileTypes;

using ACE.Entity.Enum;

using ACViewer.Enum;
using ACViewer.Extensions;
using ACViewer.Model;
using ACViewer.View;

using Matrix4x4 = System.Numerics.Matrix4x4;
using UkooLabs.FbxSharpie.Tokens.Value;
using UkooLabs.FbxSharpie.Tokens;

namespace ACViewer
{
    public static class FileExport
    {
        public static bool ExportRaw(DatType datType, uint fileID, string outFilename)
        {
            DatDatabase datDatabase = null;

            switch (datType)
            {
                case DatType.Cell:
                    datDatabase = DatManager.CellDat;
                    break;
                case DatType.Portal:
                    datDatabase = DatManager.PortalDat;
                    break;
                case DatType.HighRes:
                    datDatabase = DatManager.HighResDat;
                    break;
                case DatType.Language:
                    datDatabase = DatManager.LanguageDat;
                    break;
            }

            if (datDatabase == null) return false;

            var datReader = datDatabase.GetReaderForFile(fileID);

            if (datReader == null) return false;

            var maxFileSize = 10000000;

            using (var memoryStream = new MemoryStream(datReader.Buffer))
            {
                using (var reader = new BinaryReader(memoryStream))
                {
                    var bytes = reader.ReadBytes(maxFileSize);
                    Console.WriteLine($"Read {bytes.Length} bytes");

                    File.WriteAllBytes(outFilename, bytes);
                }
            }
            MainWindow.Instance.AddStatusText($"Wrote {outFilename}");
            return true;
        }

        public static bool ExportModel(uint fileID, string outFilename)
        {
            if (fileID >> 24 != 0x1 && fileID >> 24 != 0x2)
            {
                Console.WriteLine($"Unknown model file: {fileID:X8}");
                return false;
            }

            var sb = new StringBuilder();

            sb.AppendLine($"# {fileID:X8}");
            sb.AppendLine();
            sb.AppendLine($"mtllib {fileID:X8}.mtl");

            var surfaceIDs = new Dictionary<uint, bool>();

            var isSetup = fileID >> 24 == 0x2;

            var startIdx = 0;
            var startUVIdx = 0;

            if (isSetup)
            {
                var setup = DatManager.PortalDat.ReadFromDat<SetupModel>(fileID);

                List<Frame> placementFrames = null;

                if (setup.PlacementFrames.TryGetValue((int)Placement.Resting, out var placement) || setup.PlacementFrames.TryGetValue((int)Placement.Default, out placement))
                    placementFrames = placement.AnimFrame.Frames;

                for (var i = 0; i < setup.Parts.Count; i++)
                {
                    var part = setup.Parts[i];

                    if (part == 0x010001ec)   // skip anchor locations
                        continue;

                    var transform = Matrix4x4.Identity;

                    if (i < setup.DefaultScale.Count && setup.DefaultScale[i] != Vector3.One)
                        transform = Matrix4x4.CreateScale(setup.DefaultScale[i]);

                    if (placementFrames != null && i < placementFrames.Count)
                    {
                        var partFrame = placementFrames[i];

                        transform *= Matrix4x4.CreateFromQuaternion(partFrame.Orientation) * Matrix4x4.CreateTranslation(partFrame.Origin);
                    }

                    sb.AppendLine();
                    sb.AppendLine($"# {part:X8}");
                    sb.AppendLine();

                    ExportGfxObj(part, sb, ref startIdx, ref startUVIdx, transform, surfaceIDs);
                }
            }
            else
            {
                sb.AppendLine();
                ExportGfxObj(fileID, sb, ref startIdx, ref startUVIdx, Matrix4x4.Identity, surfaceIDs);
            }

            File.WriteAllText(outFilename, sb.ToString());
            MainWindow.Instance.AddStatusText($"Wrote {outFilename}");

            Console.Write(sb.ToString());

            Console.WriteLine();

            var fi = new System.IO.FileInfo(outFilename);
            var mtlFilename = fi.DirectoryName + Path.DirectorySeparatorChar + $"{fileID:X8}.mtl";

            ExportSurfaces(fileID, surfaceIDs, mtlFilename);

            return true;
        }

        private static void ExportGfxObj(uint gfxObjID, StringBuilder sb, ref int startIdx, ref int startUVIdx, Matrix4x4 transform, Dictionary<uint, bool> surfaceIDs)
        {
            var gfxObj = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.GfxObj>(gfxObjID);

            // vertices
            var vertices = gfxObj.VertexArray.Vertices.OrderBy(i => i.Key).Select(i => i.Value).ToList();

            // directx -> opengl / left-hand -> right-hand
            // model viewer also has y & z swapped

            // 0x020000A7 is a good test for maintaining UV's -- note the rivet locations on the top vs. sides
            // 0x02000001 is also a good test for final model orientation

            foreach (var _v in vertices)
            {
                var v = Vector3.Transform(_v.Origin, transform);
                sb.AppendLine($"v {-v.X} {v.Z} {v.Y}");
            }
            sb.AppendLine();

            // texture coordinates
            var vertexUVs = new Dictionary<VertexUV, int>();
            var nextUvIdx = 0;

            for (var i = 0; i < vertices.Count(); i++)
            {
                var v = vertices[i];

                for (var j = 0; j < v.UVs.Count; j++)
                {
                    var uv = v.UVs[j];

                    sb.AppendLine($"vt {uv.U} {-uv.V}");
                    vertexUVs.Add(new VertexUV(i, j), nextUvIdx++);
                }
            }
            sb.AppendLine();

            // normals
            foreach (var _v in gfxObj.VertexArray.Vertices.OrderBy(i => i.Key).Select(i => i.Value))
            {
                var v = Vector3.Transform(_v.Normal, transform);
                sb.AppendLine($"vn {-v.X} {v.Z} {v.Y}");
            }

            var si = startIdx;
            uint lastSurfaceId = 0;

            // polygons
            foreach (var poly in gfxObj.Polygons.OrderBy(i => i.Key).Select(i => i.Value))
            {
                var currentSurfaceId = gfxObj.Surfaces[poly.PosSurface];

                if (currentSurfaceId != lastSurfaceId)
                {
                    sb.AppendLine();
                    sb.AppendLine($"usemtl {currentSurfaceId:X8}");
                    sb.AppendLine();
                    lastSurfaceId = currentSurfaceId;
                }

                var polyStr = "f";

                for (var i = 0; i < poly.VertexIds.Count; i++)
                {
                    var v = poly.VertexIds[i];
                    var uvIdx = vertexUVs[new VertexUV(v, i < poly.PosUVIndices.Count ? poly.PosUVIndices[i] : 0)];     // investigate: some polys dont have uv arrays?
                    polyStr += $" {v + startIdx + 1}/{uvIdx + startUVIdx + 1}/{v + startIdx + 1}";
                }
                sb.AppendLine(polyStr);
            }

            startIdx += gfxObj.VertexArray.Vertices.Count;
            startUVIdx += vertexUVs.Count;

            var _gfxObj = GfxObjCache.Get(gfxObjID);

            foreach (var surfaceID in gfxObj.Surfaces)
            {
                var existing = surfaceIDs.TryGetValue(surfaceID, out var hasWrappingUVs);

                if (existing)
                {
                    if (!hasWrappingUVs && _gfxObj.HasWrappingUVs)
                        surfaceIDs[surfaceID] = true;
                }
                else
                    surfaceIDs.Add(surfaceID, _gfxObj.HasWrappingUVs);
            }
        }

        private static void ExportSurfaces(uint fileID, Dictionary<uint, bool> surfaceIDs, string outFilename)
        {
            var fi = new System.IO.FileInfo(outFilename);

            var sb = new StringBuilder();

            sb.AppendLine($"# {fileID:X8}");

            foreach (var kvp in surfaceIDs)
            {
                var surfaceID = kvp.Key;
                var hasWrappingUVs = kvp.Value;

                var surfaceFilename = fi.DirectoryName + Path.DirectorySeparatorChar + $"{surfaceID:X8}.png";

                if (!File.Exists(surfaceFilename))
                    ExportImage(surfaceID, surfaceFilename);

                //var options = hasWrappingUVs ? "" : "-clamp on ";     // doesn't work??
                var options = "";

                sb.AppendLine();
                sb.AppendLine($"newmtl {surfaceID:X8}");
                //sb.AppendLine($"Ka 1 1 1");
                //sb.AppendLine($"Kd 1 1 1");
                //sb.AppendLine($"Ks 0 0 0");
                //sb.AppendLine($"map_Ka {options}{surfaceID:X8}.png");
                sb.AppendLine($"map_Kd {options}{surfaceID:X8}.png");
                //sb.AppendLine($"map_Ks {options}{surfaceID:X8}.png");
            }

            File.WriteAllText(outFilename, sb.ToString());

            Console.WriteLine(sb.ToString());

            /*foreach (var surfaceID in surfaceIDs.Keys)
                Console.WriteLine($"Exported {surfaceID:X8}.png");

            Console.WriteLine();*/
        }

        public static bool ExportImage(uint fileID, string outFilename)
        {
            var fileType = fileID >> 24;

            if (fileType != 0x5 && fileType != 0x6 && fileType != 0x8)
            {
                Console.WriteLine($"Unknown image file: {fileID:X8}");
                return false;
            }

            if (fileType == 0x8)
            {
                var surface = DatManager.PortalDat.ReadFromDat<Surface>(fileID);
                fileID = surface.OrigTextureId;
                fileType = 0x05;
            }

            Bitmap highRes = null;

            if (fileType == 0x5)
            {
                var surfaceTexture = DatManager.PortalDat.ReadFromDat<SurfaceTexture>(fileID);

                // since previous file dialog had user enter a filename, 
                // only export highest resolution texture as that filename

                foreach (var textureID in surfaceTexture.Textures)
                {
                    var bitmap = GetBitmap(textureID);

                    if (bitmap != null && (highRes == null || bitmap.Width * bitmap.Height > highRes.Width * highRes.Height))
                        highRes = bitmap;
                }
            }
            else
                highRes = GetBitmap(fileID);

            if (highRes == null) return false;

            highRes.Save(outFilename);

            MainWindow.Instance.AddStatusText($"Wrote {outFilename}");
            return true;
        }

        private static Bitmap GetBitmap(uint textureID)
        {
            var texture = DatManager.PortalDat.ReadFromDat<Texture>(textureID);

            if (texture.Id == 0 && DatManager.HighResDat != null)
                texture = DatManager.HighResDat.ReadFromDat<Texture>(textureID);

            if (texture.Id == 0) return null;

            return texture.GetBitmap();
        }

        public static bool ExportSound(uint fileID, string outFilename)
        {
            var fileType = fileID >> 24;

            if (fileType != 0xA)
            {
                Console.WriteLine($"Unknown audio file: {fileID:X8}");
                return false;
            }
            var sound = DatManager.PortalDat.ReadFromDat<Wave>(fileID);

            using (var f = new FileStream(outFilename, FileMode.Create))
            {
                sound.ReadData(f);
                f.Close();
            }
            MainWindow.Instance.AddStatusText($"Wrote {outFilename}");
            return true;
        }

        // ===============================

        /*public static bool ExportModel_Aspose(uint fileID, MotionData motionData, string outFilename)
        {
            if (fileID >> 24 != 0x1 && fileID >> 24 != 0x2)
            {
                Console.WriteLine($"Unknown model file: {fileID:X8}");
                return false;
            }

            var scene = new Aspose.ThreeD.Scene();

            var surfaceIDs = new Dictionary<uint, bool>();

            var isSetup = fileID >> 24 == 0x2;

            if (isSetup)
            {
                var setup = DatManager.PortalDat.ReadFromDat<SetupModel>(fileID);

                List<Frame> placementFrames = null;

                if (setup.PlacementFrames.TryGetValue((int)Placement.Resting, out var placement) || setup.PlacementFrames.TryGetValue((int)Placement.Default, out placement))
                    placementFrames = placement.AnimFrame.Frames;

                for (var i = 0; i < setup.Parts.Count; i++)
                {
                    var gfxObjId = setup.Parts[i];

                    if (gfxObjId == 0x010001ec)   // skip anchor locations
                        continue;

                    var meshNode = new Aspose.ThreeD.Node($"{gfxObjId:X8}");    // check for duplicates?

                    if (i < setup.DefaultScale.Count && setup.DefaultScale[i] != Vector3.One)
                        meshNode.Transform.SetScale(setup.DefaultScale[i].X, setup.DefaultScale[i].Z, setup.DefaultScale[i].Y);

                    if (placementFrames != null && i < placementFrames.Count)
                    {
                        var partFrame = placementFrames[i];

                        meshNode.Transform.SetTranslation(-partFrame.Origin.X, partFrame.Origin.Z, partFrame.Origin.Y);

                        var q = new System.Numerics.Quaternion(-partFrame.Orientation.X, partFrame.Orientation.Z, partFrame.Orientation.Y, partFrame.Orientation.W);
                        var a = q.ToEulerAngles();

                        meshNode.Transform.EulerAngles = new Aspose.ThreeD.Utilities.Vector3(a.X.ToDegs(), a.Y.ToDegs(), a.Z.ToDegs());
                    }

                    var mesh = ExportGfxObj_Aspose(gfxObjId, Matrix4x4.Identity, surfaceIDs);

                    meshNode.Entity = mesh;

                    ExportSurfaces_Aspose(fileID, surfaceIDs, outFilename, meshNode);

                    scene.RootNode.ChildNodes.Add(meshNode);
                }
            }
            else
            {
                var mesh = ExportGfxObj_Aspose(fileID, Matrix4x4.Identity, surfaceIDs);

                var meshNode = new Aspose.ThreeD.Node($"{fileID:X8}");
                meshNode.Entity = mesh;

                ExportSurfaces_Aspose(fileID, surfaceIDs, outFilename, meshNode);

                scene.RootNode.ChildNodes.Add(meshNode);
            }

            if (motionData != null)
                BuildAnimation_Aspose(scene, motionData);

            //TrialException.SuppressTrialException = true;

            if (outFilename.EndsWith(".fbx"))
            {
                scene.Save(outFilename, FileFormat.FBX7700Binary);
                MainWindow.Instance.AddStatusText($"Wrote {outFilename}");

                //var outTextFilename = outFilename.Replace(".fbx", "-text.fbx");
                //scene.Save(outTextFilename, FileFormat.FBX7700ASCII);
                //MainWindow.Instance.AddStatusText($"Wrote {outTextFilename}");
            }
            else if (outFilename.EndsWith(".dae"))
            {
                scene.Save(outFilename, FileFormat.Collada);
                MainWindow.Instance.AddStatusText($"Wrote {outFilename}");
            }
            else
                return false;

            return true;
        }

        private static Aspose.ThreeD.Entities.Mesh ExportGfxObj_Aspose(uint gfxObjID, Matrix4x4 transform, Dictionary<uint, bool> surfaceIDs)
        {
            var gfxObj = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.GfxObj>(gfxObjID);

            // vertices
            var vertices = gfxObj.VertexArray.Vertices.OrderBy(i => i.Key).Select(i => i.Value).ToList();

            // directx -> opengl / left-hand -> right-hand
            // model viewer also has y & z swapped

            // 0x020000A7 is a good test for maintaining UV's -- note the rivet locations on the top vs. sides
            // 0x02000001 is also a good test for final model orientation

            var mesh = new Aspose.ThreeD.Entities.Mesh();

            foreach (var _v in vertices)
            {
                var v = Vector3.Transform(_v.Origin, transform);
                mesh.ControlPoints.Add(new Aspose.ThreeD.Utilities.Vector4(-v.X, v.Z, v.Y, 1.0));
            }

            // texture coordinates
            var vertexUVs = new Dictionary<VertexUV, int>();
            var nextUvIdx = 0;
            var uvs = new List<Aspose.ThreeD.Utilities.Vector4>();

            for (var i = 0; i < vertices.Count(); i++)
            {
                var v = vertices[i];

                for (var j = 0; j < v.UVs.Count; j++)
                {
                    var uv = v.UVs[j];

                    uvs.Add(new Aspose.ThreeD.Utilities.Vector4(uv.U, -uv.V, 0.0, 1.0));

                    vertexUVs.Add(new VertexUV(i, j), nextUvIdx++);
                }
            }

            // normals
            var normals = new List<Aspose.ThreeD.Utilities.Vector4>();

            foreach (var _v in gfxObj.VertexArray.Vertices.OrderBy(i => i.Key).Select(i => i.Value))
            {
                var v = Vector3.Transform(_v.Normal, transform);
                normals.Add(new Aspose.ThreeD.Utilities.Vector4(-v.X, v.Z, v.Y, 1.0));
            }

            //var si = startIdx;
            uint lastSurfaceId = 0;

            // polygons
            var builder = new PolygonBuilder(mesh);
            var uvIndices = new List<int>();
            var matIndices = new List<int>();

            foreach (var poly in gfxObj.Polygons.OrderBy(i => i.Key).Select(i => i.Value))
            {
                var currentSurfaceId = gfxObj.Surfaces[poly.PosSurface];

                if (currentSurfaceId != lastSurfaceId)
                    lastSurfaceId = currentSurfaceId;

                matIndices.Add(poly.PosSurface);

                builder.Begin();

                for (var i = 0; i < poly.VertexIds.Count; i++)
                {
                    var v = poly.VertexIds[i];
                    var uvIdx = vertexUVs[new VertexUV(v, i < poly.PosUVIndices.Count ? poly.PosUVIndices[i] : 0)];     // investigate: some polys dont have uv arrays?

                    //polyStr += $" {v + startIdx + 1}/{uvIdx + startUVIdx + 1}/{v + startIdx + 1}";
                    builder.AddVertex(v);
                    uvIndices.Add(uvIdx);
                }
                //sb.AppendLine(polyStr);
                builder.End();
            }

            var elementNormal = mesh.CreateElement(VertexElementType.Normal, MappingMode.ControlPoint, ReferenceMode.Direct) as VertexElementNormal;
            elementNormal.Data.AddRange(normals);

            var elementUV = mesh.CreateElementUV(Aspose.ThreeD.Entities.TextureMapping.Diffuse, MappingMode.PolygonVertex, ReferenceMode.IndexToDirect);
            elementUV.Data.AddRange(uvs);
            elementUV.Indices.AddRange(uvIndices);

            var elementMats = mesh.CreateElement(VertexElementType.Material, MappingMode.Polygon, ReferenceMode.IndexToDirect) as VertexElementMaterial;
            elementMats.Indices.AddRange(matIndices);

            var _gfxObj = GfxObjCache.Get(gfxObjID);

            foreach (var surfaceID in gfxObj.Surfaces)
            {
                var existing = surfaceIDs.TryGetValue(surfaceID, out var hasWrappingUVs);

                if (existing)
                {
                    if (!hasWrappingUVs && _gfxObj.HasWrappingUVs)
                        surfaceIDs[surfaceID] = true;
                }
                else
                    surfaceIDs.Add(surfaceID, _gfxObj.HasWrappingUVs);
            }

            return mesh;
        }

        private static void ExportSurfaces_Aspose(uint fileID, Dictionary<uint, bool> surfaceIDs, string outFilename, Aspose.ThreeD.Node meshNode)
        {
            var fi = new System.IO.FileInfo(outFilename);

            foreach (var kvp in surfaceIDs)
            {
                var surfaceID = kvp.Key;
                //var hasWrappingUVs = kvp.Value;

                var surfaceFilename = fi.DirectoryName + Path.DirectorySeparatorChar + $"{surfaceID:X8}.png";

                if (!File.Exists(surfaceFilename))
                    ExportImage(surfaceID, surfaceFilename);

                var diffuse = new Aspose.ThreeD.Shading.Texture();

                diffuse.FileName = $"{surfaceID:X8}.png";
                //diffuse.Content = File.ReadAllBytes(surfaceFilename);   // embed actual data, instead of linking to external filename - fbx feature only?

                var material = new Aspose.ThreeD.Shading.PhongMaterial();
                //var material = new Aspose.ThreeD.Shading.LambertMaterial();
                material.Name = $"{surfaceID:X8}";
                material.SetTexture("DiffuseColor", diffuse);
                material.SpecularFactor = 0.0f;
                material.ReflectionFactor = 0.0f;

                meshNode.Materials.Add(material);
            }

            surfaceIDs.Clear();
        }

        private static void BuildAnimation_Aspose(Aspose.ThreeD.Scene scene, MotionData motionData)
        {
            // create bindpoints
            var bpTranslate = new List<BindPoint>();
            var bpRotate = new List<BindPoint>();

            foreach (var meshNode in scene.RootNode.ChildNodes)
            {
                var translate = new BindPoint(scene, meshNode.Transform.FindProperty("Translation"));
                translate.BindKeyframeSequence("X", new KeyframeSequence());
                translate.BindKeyframeSequence("Y", new KeyframeSequence());
                translate.BindKeyframeSequence("Z", new KeyframeSequence());
                bpTranslate.Add(translate);

                //var rotate = new BindPoint(scene, meshNode.Transform.FindProperty("Rotation"));
                var rotate = new BindPoint(scene, meshNode.Transform.FindProperty("EulerAngles"));
                rotate.BindKeyframeSequence("X", new KeyframeSequence());
                rotate.BindKeyframeSequence("Y", new KeyframeSequence());
                rotate.BindKeyframeSequence("Z", new KeyframeSequence());
                //rotate.BindKeyframeSequence("W", new KeyframeSequence());
                bpRotate.Add(rotate);
            }

            var interpMode = Interpolation.Constant;

            foreach (var animData in motionData.Anims)
            {
                var perFrame = 1.0f / animData.Framerate;

                var anim = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Animation>(animData.AnimId);

                for (var animFrameIdx = 0; animFrameIdx < anim.PartFrames.Count; animFrameIdx++)
                {
                    var curTime = perFrame * animFrameIdx;
                    var partFrames = anim.PartFrames[animFrameIdx];

                    for (var partIdx = 0; partIdx < partFrames.Frames.Count; partIdx++)
                    {
                        var partFrame = partFrames.Frames[partIdx];

                        bpTranslate[partIdx].GetKeyframeSequence("X").Add(curTime, -partFrame.Origin.X, interpMode);
                        bpTranslate[partIdx].GetKeyframeSequence("Y").Add(curTime, partFrame.Origin.Z, interpMode);
                        bpTranslate[partIdx].GetKeyframeSequence("Z").Add(curTime, partFrame.Origin.Y, interpMode);

                        var q = new System.Numerics.Quaternion(-partFrame.Orientation.X, partFrame.Orientation.Z, partFrame.Orientation.Y, partFrame.Orientation.W);
                        var a = q.ToEulerAngles();

                        bpRotate[partIdx].GetKeyframeSequence("X").Add(curTime, a.X.ToDegs(), interpMode);
                        bpRotate[partIdx].GetKeyframeSequence("Y").Add(curTime, a.Y.ToDegs(), interpMode);
                        bpRotate[partIdx].GetKeyframeSequence("Z").Add(curTime, a.Z.ToDegs(), interpMode);
                        //bpRotate[partIdx].GetKeyframeSequence("W").Add(curTime, partFrame.Orientation.W, interpMode);
                    }
                }
            }
        }*/

        // ===============================

        public static bool ExportModel_Assimp(uint fileID, MotionData motionData, string outFilename)
        {
            if (fileID >> 24 != 0x1 && fileID >> 24 != 0x2)
            {
                Console.WriteLine($"Unknown model file: {fileID:X8}");
                return false;
            }

            var scene = new Assimp.Scene();
            scene.RootNode = new Assimp.Node();

            var isSetup = fileID >> 24 == 0x2;

            var materialIdx = new Dictionary<uint, MaterialIdx>();

            // assimp animations don't link up to mesh nodes by id or hierarchy,
            // and must link up by unique node names!
            var gfxObjIdCnts = new Dictionary<uint, int>();

            if (isSetup)
            {
                var setup = DatManager.PortalDat.ReadFromDat<SetupModel>(fileID);

                List<Frame> placementFrames = null;

                if (setup.PlacementFrames.TryGetValue((int)Placement.Resting, out var placement) || setup.PlacementFrames.TryGetValue((int)Placement.Default, out placement))
                    placementFrames = placement.AnimFrame.Frames;

                for (var i = 0; i < setup.Parts.Count; i++)
                {
                    var gfxObjId = setup.Parts[i];

                    //if (gfxObjId == 0x010001ec)   // skip anchor locations
                        //continue;

                    if (!gfxObjIdCnts.TryGetValue(gfxObjId, out var gfxObjIdCnt))
                        gfxObjIdCnts[gfxObjId] = 1;
                    else
                        gfxObjIdCnts[gfxObjId] = ++gfxObjIdCnt;

                    var meshNodeName = $"{gfxObjId:X8}";

                    if (gfxObjIdCnt > 1)
                        meshNodeName += "." + gfxObjIdCnt.ToString().PadLeft(3, '0');

                    var meshNode = new Assimp.Node(meshNodeName);

                    var transform = Matrix4x4.Identity;

                    if (i < setup.DefaultScale.Count && setup.DefaultScale[i] != Vector3.One)
                    {
                        transform = Matrix4x4.CreateScale(setup.DefaultScale[i].X, setup.DefaultScale[i].Z, setup.DefaultScale[i].Y);
                    }

                    if (placementFrames != null && i < placementFrames.Count)
                    {
                        var partFrame = placementFrames[i];

                        var rotate = new System.Numerics.Quaternion(-partFrame.Orientation.X, partFrame.Orientation.Z, partFrame.Orientation.Y, partFrame.Orientation.W);
                        var translate = new Vector3(-partFrame.Origin.X, partFrame.Origin.Z, partFrame.Origin.Y);

                        transform *= Matrix4x4.CreateFromQuaternion(rotate) * Matrix4x4.CreateTranslation(translate);
                    }

                    if (transform != Matrix4x4.Identity)
                    {
                        meshNode.Transform = new Assimp.Matrix4x4(
                            transform.M11, transform.M21, transform.M31, transform.M41,
                            transform.M12, transform.M22, transform.M32, transform.M42,
                            transform.M13, transform.M23, transform.M33, transform.M43,
                            transform.M14, transform.M24, transform.M34, transform.M44);
                    }

                    var meshes = ExportGfxObj_Assimp(gfxObjId, materialIdx);

                    foreach (var mesh in meshes)
                    {
                        scene.Meshes.Add(mesh);
                        meshNode.MeshIndices.Add(scene.Meshes.Count - 1);
                    }

                    scene.RootNode.Children.Add(meshNode);
                }
            }
            else
            {
                var meshNode = new Assimp.Node($"{fileID:X8}");

                var meshes = ExportGfxObj_Assimp(fileID, materialIdx);

                foreach (var mesh in meshes)
                {
                    scene.Meshes.Add(mesh);
                    meshNode.MeshIndices.Add(scene.Meshes.Count - 1);
                }

                scene.RootNode.Children.Add(meshNode);
            }

            ExportSurfaces_Assimp(outFilename, scene, materialIdx);

            if (motionData != null)
                BuildAnimation_Assimp(scene, motionData);

            using (var ctx = new AssimpContext())
            {
                var mesh = scene.Meshes[0];

                if (outFilename.EndsWith(".fbx"))
                {
                    if (ctx.ExportFile(scene, outFilename, "fbx"))

                        MainWindow.Instance.AddStatusText($"Wrote {outFilename}");
                    else
                        MainWindow.Instance.AddStatusText($"Failed to export {outFilename}");

                    /*var outTextFilename = outFilename.Replace(".fbx", "-text.fbx");

                    if (ctx.ExportFile(scene, outTextFilename, "fbxa"))
                        MainWindow.Instance.AddStatusText($"Wrote {outTextFilename}");
                    else
                        MainWindow.Instance.AddStatusText($"Failed to export {outTextFilename}");*/

                    FixFBX(outFilename);
                }
                else if (outFilename.EndsWith(".dae"))
                {
                    if (ctx.ExportFile(scene, outFilename, "collada"))
                        MainWindow.Instance.AddStatusText($"Wrote {outFilename}");
                    else
                        MainWindow.Instance.AddStatusText($"Failed to export {outFilename}");
                }
                else
                    return false;
            }
            return true;
        }

        private static List<Assimp.Mesh> ExportGfxObj_Assimp(uint gfxObjID, Dictionary<uint, MaterialIdx> materials)
        {
            // assimp meshes must be split up by material
            // possibly look into adding multiple textures to 1 material?
            var meshes = new Dictionary<uint, Assimp.Mesh>();

            var gfxObj = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.GfxObj>(gfxObjID);

            // vertices
            var vertices = gfxObj.VertexArray.Vertices.OrderBy(i => i.Key).Select(i => i.Value).ToList();

            // normals
            var normals = gfxObj.VertexArray.Vertices.OrderBy(i => i.Key).Select(i => i.Value).ToList();

            // texture coordinates
            var vertexUVs = new Dictionary<VertexUV, Vector2D>();

            for (var i = 0; i < vertices.Count(); i++)
            {
                var v = vertices[i];

                for (var j = 0; j < v.UVs.Count; j++)
                {
                    var uv = v.UVs[j];

                    vertexUVs.Add(new VertexUV(i, j), new Vector2D(uv.U, uv.V));
                }
            }

            // polygons
            foreach (var poly in gfxObj.Polygons.OrderBy(i => i.Key).Select(i => i.Value))
            {
                var currentSurfaceId = gfxObj.Surfaces[poly.PosSurface];

                if (!meshes.TryGetValue(currentSurfaceId, out var mesh))
                {
                    mesh = new Assimp.Mesh();
                    if (!materials.TryGetValue(currentSurfaceId, out var materialIdx))
                    {
                        materialIdx = new MaterialIdx(materials.Count);
                        materials.Add(currentSurfaceId, materialIdx);
                    }
                    mesh.MaterialIndex = materialIdx.MaterialId;
                    mesh.UVComponentCount[0] = 2;
                    meshes.Add(currentSurfaceId, mesh);
                }

                var face = new Assimp.Face();

                for (var i = 0; i < poly.VertexIds.Count; i++)
                {
                    var origVertIdx = poly.VertexIds[i];
                    var v = vertices[origVertIdx];
                    var n = normals[origVertIdx];
                    var uv = vertexUVs[new VertexUV(origVertIdx, i < poly.PosUVIndices.Count ? poly.PosUVIndices[i] : 0)];     // investigate: some polys dont have uv arrays?

                    // denormalize for assimp :(
                    mesh.Vertices.Add(new Vector3D(-v.Origin.X, v.Origin.Z, v.Origin.Y));
                    face.Indices.Add(mesh.Vertices.Count - 1);
                    mesh.Normals.Add(new Vector3D(-n.Normal.X, n.Normal.Z, n.Normal.Y));
                    mesh.TextureCoordinateChannels[0].Add(new Vector3D(uv.X, -uv.Y, 0.0f));
                }

                mesh.Faces.Add(face);
            }

            return meshes.Values.ToList();
        }

        private static void ExportSurfaces_Assimp(string outFilename, Assimp.Scene scene, Dictionary<uint, MaterialIdx> materialIdx)
        {
            var fi = new System.IO.FileInfo(outFilename);

            foreach (var kvp in materialIdx.OrderBy(i => i.Value.MaterialId))
            {
                var surfaceID = kvp.Key;
                var materialId = kvp.Value.MaterialId;

                var surfaceFilename = fi.DirectoryName + Path.DirectorySeparatorChar + $"{surfaceID:X8}.png";

                if (!File.Exists(surfaceFilename))
                    ExportImage(surfaceID, surfaceFilename);

                var material = new Assimp.Material();
                material.Name = $"{surfaceID:X8}";
                material.TextureDiffuse = new Assimp.TextureSlot()
                {
                    //FilePath = surfaceFilename,
                    FilePath = $"{surfaceID:X8}.png",
                    TextureType = TextureType.Diffuse,
                    //WrapModeU = TextureWrapMode.Wrap,
                    //WrapModeV = TextureWrapMode.Wrap,
                };

                // if this is 0, assimp defaults to lambert shading
                // even forcing phong shading seems to be ignored. this must be set...
                material.Shininess = 0.00001f;

                // there seems to be no other way to set some important material props via assimp currently :(
                // going to use this as a base, and then fill in the rest via raw fbx reading/writing via UkooLabs.FbxSharpie

                scene.Materials.Add(material);
            }
        }

        private static void BuildAnimation_Assimp(Assimp.Scene scene, MotionData motionData)
        {
            var animation = new Assimp.Animation();
            animation.Name = "ACAnim";

            foreach (var animData in motionData.Anims)
            {
                var perFrame = 1.0f / animData.Framerate;

                var anim = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Animation>(animData.AnimId);

                animation.DurationInTicks = perFrame * anim.PartFrames.Count;
                animation.TicksPerSecond = 1.0;

                for (var animFrameIdx = 0; animFrameIdx < anim.PartFrames.Count; animFrameIdx++)
                {
                    var curTime = perFrame * animFrameIdx;
                    var partFrames = anim.PartFrames[animFrameIdx];

                    for (var partIdx = 0; partIdx < partFrames.Frames.Count; partIdx++)
                    {
                        var partFrame = partFrames.Frames[partIdx];
                        var meshNode = scene.RootNode.Children[partIdx];

                        NodeAnimationChannel nodeAnimationChannel = null;

                        if (partIdx < animation.NodeAnimationChannels.Count)
                        {
                            nodeAnimationChannel = animation.NodeAnimationChannels[partIdx];
                        }
                        else
                        {
                            nodeAnimationChannel = new NodeAnimationChannel();
                            nodeAnimationChannel.NodeName = meshNode.Name;

                            animation.NodeAnimationChannels.Add(nodeAnimationChannel);
                        }

                        nodeAnimationChannel.PositionKeys.Add(new VectorKey(curTime, new Vector3D(-partFrame.Origin.X, partFrame.Origin.Z, partFrame.Origin.Y)));
                        nodeAnimationChannel.RotationKeys.Add(new QuaternionKey(curTime, new Assimp.Quaternion(partFrame.Orientation.W, -partFrame.Orientation.X, partFrame.Orientation.Z, partFrame.Orientation.Y)));
                    }
                }
            }
            scene.Animations.Add(animation);

            // multiple anims "work", but managing them in blender feels very unruly atm...
            // only doing single animations for now, until this is figured out better
        }

        private static void FixFBX(string filename)
        {
            var fbx = FbxIO.Read(filename);

            ShowNodes(fbx.Nodes, null);

            FbxIO.WriteBinary(fbx, filename);
        }

        private static readonly bool Debug = false;

        private static void ShowNodes(FbxNode[] nodes, FbxNode parent, int level = 0)
        {
            var prefix = "".PadLeft(level * 2, ' ');

            foreach (var node in nodes)
            {
                if (node == null) continue;

                if (Debug) Console.WriteLine(prefix + node.Identifier.Value);

                if (node.Properties.Length > 0)
                {
                    //Console.WriteLine($"Properties:");
                    foreach (var prop in node.Properties)
                    {
                        if (prop is StringToken st)
                        {
                            if (Debug) Console.WriteLine($"{prefix} - {st.Value}");

                            if (st.Value == "UnitScaleFactor")
                            {
                                if (node.Properties.LastOrDefault() is DoubleToken unitScaleFactor)
                                {
                                    //Console.WriteLine($"Fixed UnitScaleFactor {unitScaleFactor.Value} -> 100");
                                    unitScaleFactor.Value = 100.0;
                                }
                                else
                                    Console.WriteLine($"Found UnitScaleFactor, but not fixed!");
                            }
                            else if (st.Value == "Shininess")
                            {
                                if (node.Properties.LastOrDefault() is DoubleToken shininess)
                                {
                                    //Console.WriteLine($"Fixed Shininess {shininess.Value} -> 0");
                                    shininess.Value = 0.0;
                                }
                                else
                                    Console.WriteLine($"Found Shininess, but not fixed!");

                                var specularFactor = new FbxNode(new IdentifierToken("P"));
                                specularFactor.AddProperty(new StringToken("SpecularFactor"));
                                specularFactor.AddProperty(new StringToken("Number"));
                                specularFactor.AddProperty(new StringToken(""));
                                specularFactor.AddProperty(new StringToken("A"));
                                specularFactor.AddProperty(new DoubleToken(0));
                                parent.AddNode(specularFactor);

                                var reflectionFactor = new FbxNode(new IdentifierToken("P"));
                                reflectionFactor.AddProperty(new StringToken("ReflectionFactor"));
                                reflectionFactor.AddProperty(new StringToken("Number"));
                                reflectionFactor.AddProperty(new StringToken(""));
                                reflectionFactor.AddProperty(new StringToken("A"));
                                reflectionFactor.AddProperty(new DoubleToken(0));
                                parent.AddNode(reflectionFactor);
                            }
                            else if (st.Value == "ShininessExponent")
                            {
                                if (node.Properties.LastOrDefault() is DoubleToken shininessExponent)
                                {
                                    if (shininessExponent.Value < 20)
                                    {
                                        //Console.WriteLine($"Fixed ShininessExponent {shininessExponent.Value} -> 0");
                                        shininessExponent.Value = 0.0;
                                    }
                                }
                                else
                                    Console.WriteLine($"Found ShininessExponent, but not fixed!");
                            }
                        }
                        else if (prop is IntegerToken it)
                        {
                            if (Debug) Console.WriteLine($"{prefix} - {it.Value}");
                        }
                        else if (prop is FloatToken ft)
                        {
                            if (Debug) Console.WriteLine($"{prefix} - {ft.Value}");
                        }
                        else if (prop is BooleanToken bt)
                        {
                            if (Debug) Console.WriteLine($"{prefix} - {bt.Value}");
                        }
                        else if (prop is DoubleToken dt)
                        {
                            if (Debug) Console.WriteLine($"{prefix} - {dt.Value}");
                        }
                        else if (prop is LongToken lt)
                        {
                            if (Debug) Console.WriteLine($"{prefix} - {lt.Value}");
                        }
                        else if (prop is ShortToken ht)
                        {
                            if (Debug) Console.WriteLine($"{prefix} - {ht.Value}");
                        }
                    }
                }

                if (node.Nodes.Length > 0)
                    ShowNodes(node.Nodes, node, level + 1);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

using ACE.DatLoader;
using ACE.DatLoader.Entity;
using ACE.DatLoader.FileTypes;

using ACE.Entity.Enum;

using ACViewer.Enum;
using ACViewer.Model;
using ACViewer.View;

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
    }
}

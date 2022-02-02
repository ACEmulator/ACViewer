using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.Server.Physics.Common;

namespace ACViewer.Render
{
    public class TerrainBatchDraw
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public static Effect Effect => Render.Effect_Clamp;

        public TextureAtlasChain OverlayAtlasChain { get; set; }

        public TextureAtlasChain AlphaAtlasChain { get; set; }

        public List<LandVertex> Vertices { get; set; }

        public VertexBuffer VertexBuffer { get; set; }

        public int NumItems { get; set; }

        public const int SizeOfLandVertexPlus = sizeof(float) * 29;  // can't find a way to get this statically -- change this if sizeof(LandVertexPlus) ever changes

        public const int MaxVertices = int.MaxValue / SizeOfLandVertexPlus;   // 18,512,790 vertices

        public TerrainBatchDraw(TextureAtlasChain overlayAtlasChain, TextureAtlasChain alphaAtlasChain)
        {
            OverlayAtlasChain = overlayAtlasChain;

            AlphaAtlasChain = alphaAtlasChain;

            Init();
        }

        public void Init()
        {
            Vertices = new List<LandVertex>();
        }

        public bool CanAdd(R_Landblock landblock)
        {
            return Vertices.Count + landblock.Landblock.Polygons.Count * 3 <= MaxVertices;
        }

        public void AddTerrain(R_Landblock landblock)
        {
            foreach (var poly in landblock.Landblock.Polygons)
            {
                var surfnum = (uint)poly.PosSurface;

                var surface = LandSurf.Instance.GetLandSurface(surfnum);
                var surfInfo = surface.Info;

                for (var i = 0; i < 3; i++)
                {
                    var v = StartVertex(surfInfo);

                    var vertex = poly.Vertices[i];
                    var uvIdx = poly.PosUVIndices[i];
                    var uv = LandblockStruct.LandUVs[uvIdx];

                    // pre-transform into world space
                    v.Position = Vector3.Transform(vertex.Origin.ToXna(), landblock.WorldTransform);
                    v.Normal = vertex.Normal.ToXna();

                    v.TexCoord0 = new Vector3(uv.X, uv.Y, v.TexCoord0.Z);

                    var numTerrains = surfInfo.TerrainRotations.Count;

                    if (numTerrains > 0)
                    {
                        var rot = LandblockStruct.LandUVsRotated[(byte)surfInfo.TerrainRotations[0]][uvIdx];
                        v.TexCoord1 = new Vector4(rot.X, rot.Y, v.TexCoord1.Z, v.TexCoord1.W);
                    }
                    if (numTerrains > 1)
                    {
                        var rot = LandblockStruct.LandUVsRotated[(byte)surfInfo.TerrainRotations[1]][uvIdx];
                        v.TexCoord2 = new Vector4(rot.X, rot.Y, v.TexCoord2.Z, v.TexCoord2.W);
                    }
                    if (numTerrains > 2)
                    {
                        var rot = LandblockStruct.LandUVsRotated[(byte)surfInfo.TerrainRotations[2]][uvIdx];
                        v.TexCoord3 = new Vector4(rot.X, rot.Y, v.TexCoord3.Z, v.TexCoord3.W);
                    }

                    var numRoads = surfInfo.RoadRotations.Count;

                    if (numRoads > 0)
                    {
                        var rot = LandblockStruct.LandUVsRotated[(byte)surfInfo.RoadRotations[0]][uvIdx];
                        v.TexCoord4 = new Vector4(rot.X, rot.Y, v.TexCoord4.Z, v.TexCoord4.W);
                    }
                    if (numRoads > 1)
                    {
                        var rot = LandblockStruct.LandUVsRotated[(byte)surfInfo.RoadRotations[1]][uvIdx];
                        v.TexCoord5 = new Vector4(rot.X, rot.Y, v.TexCoord5.Z, v.TexCoord5.W);
                    }

                    Vertices.Add(v);
                }
            }
        }

        private LandVertex StartVertex(TextureMergeInfo surfInfo)
        {
            var v = new LandVertex(true);

            // base terrain
            var texID = surfInfo.TerrainBase.TexGID;

            var overlayIdx = OverlayAtlasChain.GetTextureIdx(texID);

            v.TexCoord0.Z = overlayIdx;

            // terrain overlays (max 3)
            for (var i = 0; i < surfInfo.TerrainOverlays.Count; i++)
            {
                var overlayID = surfInfo.TerrainOverlays[i].TexGID;

                overlayIdx = OverlayAtlasChain.GetTextureIdx(overlayID);

                if (i == 0)
                    v.TexCoord1.Z = overlayIdx;
                else if (i == 1)
                    v.TexCoord2.Z = overlayIdx;
                else if (i == 2)
                    v.TexCoord3.Z = overlayIdx;
            }

            // road overlay
            if (surfInfo.RoadOverlay != null)
            {
                var overlayID = surfInfo.RoadOverlay.TexGID;

                overlayIdx = OverlayAtlasChain.GetTextureIdx(overlayID);

                v.TexCoord4.Z = overlayIdx;

                if (surfInfo.RoadAlphaOverlays.Count > 1)
                    v.TexCoord5.Z = overlayIdx;
            }

            // terrain alphas (max 3)
            for (var i = 0; i < surfInfo.TerrainAlphaOverlays.Count; i++)
            {
                var alphaID = surfInfo.TerrainAlphaOverlays[i].TexGID;

                var alphaIdx = AlphaAtlasChain.GetTextureIdx(alphaID);

                if (i == 0)
                    v.TexCoord1.W = alphaIdx;
                else if (i == 1)
                    v.TexCoord2.W = alphaIdx;
                else if (i == 2)
                    v.TexCoord3.W = alphaIdx;
            }

            // road alphas (max 2)
            for (var i = 0; i < surfInfo.RoadAlphaOverlays.Count; i++)
            {
                var alphaID = surfInfo.RoadAlphaOverlays[i].RoadTexGID;

                var alphaIdx = AlphaAtlasChain.GetTextureIdx(alphaID);

                if (i == 0)
                    v.TexCoord4.W = alphaIdx;
                else if (i == 1)
                    v.TexCoord5.W = alphaIdx;
            }

            return v;
        }

        public void OnCompleted()
        {
            BuildBuffer();
        }

        private void BuildBuffer()
        {
            if (Vertices.Count == 0)
                return;

            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(LandVertex), Vertices.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData(Vertices.ToArray());

            NumItems = Vertices.Count / 3;
        }

        public void Draw()
        {
            if (VertexBuffer == null) return;

            GraphicsDevice.SetVertexBuffer(VertexBuffer);

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, NumItems);
            }
        }

        public void Dispose()
        {
            if (VertexBuffer != null)
                VertexBuffer.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Server.Physics.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

namespace ACViewer.Render
{
    public class R_Landblock
    {
        public static GraphicsDevice GraphicsDevice { get => GameView.Instance.GraphicsDevice; }

        public static Effect Effect { get => Render.Effect; }

        public Landblock Landblock;

        //public List<VertexPositionColor> W_Vertices;
        //public List<short> Indices;

        public List<LandVertex> Vertices;

        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;

        public List<R_PhysicsObj> StaticObjs;
        public List<R_PhysicsObj> Buildings;
        public List<R_PhysicsObj> Scenery;

        public List<R_EnvCell> EnvCells;

        public Matrix WorldTransform;

        public static bool OutdoorEnvCells = true;

        public static Dictionary<uint, Texture2D> LandOverlays;
        public static Dictionary<uint, Texture2D> LandAlphas;

        static R_Landblock()
        {
            Init();
        }

        public static void Init()
        {
            LandOverlays = new Dictionary<uint, Texture2D>();
            LandAlphas = new Dictionary<uint, Texture2D>();
        }

        public R_Landblock(Landblock landblock)
        {
            Landblock = landblock;

            WorldTransform = GetWorldTransform();

            if (!landblock.IsDungeon)
            {
                BuildVertices();
                //BuildIndices();

                //BuildBuffer();

                BuildBuildings();
                BuildStaticObjs();
                BuildScenery();

                BuildTextures();

                WorldViewer.Instance.Buffer.AddOutdoor(this);
            }

            if (landblock.IsDungeon || OutdoorEnvCells)
            {
                BuildEnvCells();
                WorldViewer.Instance.Buffer.AddEnvCells(this);
            }
        }

        public void BuildVertices()
        {
            // outdoor landblock terrain vertices
            //W_Vertices = new List<VertexPositionColor>(Landblock.VertexArray.Vertices.Count);

            //foreach (var vert in Landblock.VertexArray.Vertices)
            //W_Vertices.Add(new VertexPositionColor(vert.Origin.ToXna(), Color.White));

            //Vertices = new List<VertexPositionNormalTexture>(Landblock.Polygons.Count * 3);
            Vertices = new List<LandVertex>(Landblock.Polygons.Count * 3);

            foreach (var terrainPoly in Landblock.Polygons)
            {
                var surface = LandSurf.Instance.GetLandSurface((uint)terrainPoly.PosSurface);
                var surfInfo = surface.Info;

                for (var i = 0; i < 3; i++)
                {
                    var vertex = terrainPoly.Vertices[i];
                    var uvIdx = terrainPoly.PosUVIndices[i];
                    var uv = LandblockStruct.LandUVs[uvIdx];

                    var v = new LandVertex();

                    // pre-transform into world space
                    v.Position = Vector3.Transform(vertex.Origin.ToXna(), WorldTransform);
                    v.Normal = vertex.Normal.ToXna();
                    v.TexCoord0 = uv.ToXna();
                    v.TexCoord1 = -Vector3.One;
                    v.TexCoord2 = -Vector3.One;
                    v.TexCoord3 = -Vector3.One;
                    v.TexCoord4 = -Vector3.One;
                    v.TexCoord5 = -Vector3.One;

                    var numTerrains = surfInfo.TerrainRotations.Count;
                    if (numTerrains > 0)
                        v.TexCoord1 = LandblockStruct.LandUVsRotated[(byte)surfInfo.TerrainRotations[0]][uvIdx].ToXna(1);
                    if (numTerrains > 1)
                        v.TexCoord2 = LandblockStruct.LandUVsRotated[(byte)surfInfo.TerrainRotations[1]][uvIdx].ToXna(2);
                    if (numTerrains > 2)
                        v.TexCoord3 = LandblockStruct.LandUVsRotated[(byte)surfInfo.TerrainRotations[2]][uvIdx].ToXna(3);

                    var numRoads = surfInfo.RoadRotations.Count;
                    if (numRoads > 0)
                        v.TexCoord4 = LandblockStruct.LandUVsRotated[(byte)surfInfo.RoadRotations[0]][uvIdx].ToXna(4);
                    if (numRoads > 1)
                        v.TexCoord5 = LandblockStruct.LandUVsRotated[(byte)surfInfo.RoadRotations[1]][uvIdx].ToXna(5);

                    Vertices.Add(v);
                }
            }
        }

        public void BuildIndices()
        {
            /*Indices = new List<short>();

            // triangle list
            foreach (var poly in Landblock.Polygons)
                Indices.AddRange(poly.VertexIDs);*/
        }

        public void BuildBuffer()
        {
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(LandVertex), Vertices.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData(Vertices.ToArray());

            /*W_VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), W_VertexBuffer.Count, BufferUsage.WriteOnly);
            W_VertexBuffer.SetData(W_Vertices.ToArray());

            IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), Indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices.ToArray());*/
        }

        public void BuildStaticObjs()
        {
            StaticObjs = new List<R_PhysicsObj>();

            foreach (var staticObj in Landblock.StaticObjects)
                StaticObjs.Add(new R_PhysicsObj(staticObj));
        }

        public void BuildBuildings()
        {
            Buildings = new List<R_PhysicsObj>();

            foreach (var building in Landblock.Buildings)
                Buildings.Add(new R_PhysicsObj(building));
        }

        public void BuildScenery()
        {
            Scenery = new List<R_PhysicsObj>();

            foreach (var scenery in Landblock.Scenery)
                Scenery.Add(new R_PhysicsObj(scenery));
        }

        public void BuildTextures()
        {
            foreach (var poly in Landblock.Polygons)
            {
                var surfnum = (uint)poly.PosSurface;
                if (LandOverlays.ContainsKey(surfnum))
                    continue;

                var surface = LandSurf.Instance.GetLandSurface(surfnum);
                var surfInfo = surface.Info;

                var texID = surfInfo.TerrainBase.TexGID;
                var baseTexture = TextureCache.Get(texID);
                var numLevels = baseTexture.LevelCount;
                var numColors = baseTexture.Width * baseTexture.Height;
                //var data = new Color[numColors];
                //baseTexture.GetData(data, 0, numColors);

                var mipData = baseTexture.GetMipData(numColors);

                var overlays = new Texture2D(GraphicsDevice, baseTexture.Width, baseTexture.Height, true, SurfaceFormat.Color, 5);
                //overlays.SetData(0, 0, null, data, 0, numColors);
                for (var j = 0; j < numLevels; j++)
                    overlays.SetData(j, 0, null, mipData[j], 0, mipData[j].Length);

                // terrain overlays
                for (var i = 0; i < surfInfo.TerrainOverlays.Count; i++)
                {
                    var overlayID = surfInfo.TerrainOverlays[i].TexGID;
                    var overlayTexture = TextureCache.Get(overlayID);
                    //data = new Color[numColors];
                    //overlayTexture.GetData(data, 0, numColors);
                    mipData = overlayTexture.GetMipData(numColors);

                    //overlays.SetData(0, i + 1, null, data, 0, numColors);
                    for (var j = 0; j < numLevels; j++)
                        overlays.SetData(j, i + 1, null, mipData[j], 0, mipData[j].Length);
                }

                // road overlay
                if (surfInfo.RoadOverlay != null)
                {
                    var overlayID = surfInfo.RoadOverlay.TexGID;
                    var overlayTexture = TextureCache.Get(overlayID);
                    //data = new Color[numColors];
                    //overlayTexture.GetData(data, 0, numColors);
                    mipData = overlayTexture.GetMipData(numColors);

                    //overlays.SetData(0, 4, null, data, 0, numColors);
                    for (var j = 0; j < numLevels; j++)
                        overlays.SetData(j, 4, null, mipData[j], 0, mipData[j].Length);
                }

                //Console.WriteLine($"Adding {surfnum}");
                LandOverlays.Add(surfnum, overlays);

                var alphas = new Texture2D(GraphicsDevice, baseTexture.Width, baseTexture.Height, false, SurfaceFormat.Alpha8, 6);

                // terrain alphas
                for (var i = 0; i < surfInfo.TerrainAlphaOverlays.Count; i++)
                {
                    var alphaID = surfInfo.TerrainAlphaOverlays[i].TexGID;
                    var alphaTexture = TextureCache.Get(alphaID);
                    var alphaData = new byte[numColors];
                    alphaTexture.GetData(alphaData, 0, numColors);

                    alphas.SetData(0, i + 1, null, alphaData, 0, numColors);
                }

                // road alphas
                for (var i = 0; i < surfInfo.RoadAlphaOverlays.Count; i++)
                {
                    var alphaID = surfInfo.RoadAlphaOverlays[i].RoadTexGID;
                    var alphaTexture = TextureCache.Get(alphaID);
                    var alphaData = new byte[numColors];
                    alphaTexture.GetData(alphaData, 0, numColors);

                    alphas.SetData(0, i + 4, null, alphaData, 0, numColors);
                }

                LandAlphas.Add(surfnum, alphas);
            }
        }

        public void BuildEnvCells()
        {
            EnvCells = new List<R_EnvCell>();

            if (Landblock.Info == null || Landblock.Info.NumCells == 0)
                return;

            var numCells = Landblock.Info.NumCells;

            var landblockID = Landblock.ID & 0xFFFF0000;

            for (uint i = 0; i < numCells; i++)
            {
                var envCellID = landblockID | (0x100 + i);

                var envCell = (ACE.Server.Physics.Common.EnvCell)LScape.get_landcell(envCellID);

                EnvCells.Add(new R_EnvCell(envCell));
            }
        }

        public void Draw()
        {
            Effect.CurrentTechnique = Effect.Techniques["ColoredNoShading"];
            Effect.Parameters["xWorld"].SetValue(WorldTransform);

            if (!Landblock.IsDungeon)
            {
                DrawLand();
                DrawStaticObjs();
                DrawBuildings();
                DrawScenery();
            }

            if (Landblock.IsDungeon || OutdoorEnvCells)
                DrawEnvCells();
        }

        public void DrawLand()
        {
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.Count / 3);
                DrawCount.NumLandblock++;
            }
        }

        public void DrawStaticObjs()
        {
            foreach (var staticObj in StaticObjs)
                staticObj.Draw(WorldTransform);
        }

        public void DrawBuildings()
        {
            foreach (var building in Buildings)
                building.Draw(WorldTransform);
        }

        public void DrawScenery()
        {
            foreach (var scenery in Scenery)
                scenery.Draw(WorldTransform);
        }

        public void DrawEnvCells()
        {
            foreach (var envCell in EnvCells)
                envCell.Draw(WorldTransform);
        }

        public Matrix GetWorldTransform()
        {
            var x = Landblock.ID >> 24;
            var y = Landblock.ID >> 16 & 0xFF;

            return Matrix.CreateTranslation(new Vector3(x * LandDefs.BlockLength, y * LandDefs.BlockLength, 0));
        }
    }
}

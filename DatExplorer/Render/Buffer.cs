using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DatExplorer.Render
{
    public class Buffer
    {
        public static GraphicsDevice GraphicsDevice { get => GameView.Instance.GraphicsDevice; }

        public Dictionary<uint, TerrainBatch> TerrainGroups;   // key: surfnum
        public Dictionary<uint, RenderBatch> RB_EnvCell;       // key: surface id
        public Dictionary<uint, RenderBatch> RB_StaticObjs;
        public Dictionary<uint, RenderBatch> RB_Buildings;
        public Dictionary<uint, RenderBatch> RB_Scenery;

        public static Effect Effect { get => Render.Effect; }

        public Buffer()
        {
            Init();
        }

        public void Init()
        {
            TerrainGroups = new Dictionary<uint, TerrainBatch>();
            RB_EnvCell = new Dictionary<uint, RenderBatch>();
            RB_StaticObjs = new Dictionary<uint, RenderBatch>();
            RB_Buildings = new Dictionary<uint, RenderBatch>();
            RB_Scenery = new Dictionary<uint, RenderBatch>();
        }

        public void ClearBuffer()
        {
            foreach (var batch in TerrainGroups.Values)
                batch.Dispose();

            ClearBuffer(RB_EnvCell);
            ClearBuffer(RB_StaticObjs);
            ClearBuffer(RB_Buildings);
            ClearBuffer(RB_Scenery);

            Init();
        }

        public void ClearBuffer(Dictionary<uint, RenderBatch> batches)
        {
            foreach (var batch in batches.Values)
                batch.Dispose();
        }

        public void AddOutdoor(R_Landblock landblock)
        {
            AddTerrain(landblock);

            AddStaticObjs(landblock);
            AddBuildings(landblock);
            AddScenery(landblock);
        }

        public void AddTerrain(R_Landblock landblock)
        {
            var vertices = landblock.Vertices;
            var polygons = landblock.Landblock.Polygons;

            for (var i = 0; i < polygons.Count; i += 2)
            {
                var poly = polygons[i];

                var surfnum = (uint)poly.PosSurface;

                TerrainGroups.TryGetValue(surfnum, out var terrainGroup);

                if (terrainGroup == null)
                {
                    terrainGroup = new TerrainBatch(surfnum);
                    TerrainGroups.Add(surfnum, terrainGroup);
                }
                terrainGroup.AddCell(vertices, i);
            }
        }

        public void AddStaticObjs(R_Landblock landblock)
        {
            foreach (var obj in landblock.StaticObjs)
                AddStaticObj(obj, RB_StaticObjs);
        }

        public void AddStaticObj(R_PhysicsObj obj, Dictionary<uint, RenderBatch> batches)
        {
            var setupInstance = obj.Setup;
            var setup = setupInstance.Setup;

            if (setup.Parts.Count != obj.PartArray.Parts.Count) return;

            for (var i = 0; i < setup.Parts.Count; i++)
            {
                var part = setup.Parts[i];
                var vertices = part.VertexArray;

                var frame = obj.PartArray.Parts[i].PhysicsPart.Pos;
                var transform = frame.ToXna();

                foreach (var polygon in part.Polygons)
                {
                    var surfaceIdx = polygon._polygon.PosSurface;
                    var surfaceID = part._gfxObj.Surfaces[surfaceIdx];

                    batches.TryGetValue(surfaceID, out var batch);

                    if (batch == null)
                    {
                        var texture = part.Textures[surfaceIdx];
                        batch = new RenderBatch(texture);
                        batches.Add(surfaceID, batch);
                    }

                    batch.AddPolygon(vertices, polygon, transform);
                }
            }
        }

        public void AddBuildings(R_Landblock landblock)
        {
            foreach (var building in landblock.Buildings)
                AddStaticObj(building, RB_Buildings);
        }

        public void AddScenery(R_Landblock landblock)
        {
            foreach (var scenery in landblock.Scenery)
                AddStaticObj(scenery, RB_Scenery);
        }

        public void AddEnvCells(R_Landblock landblock)
        {
            foreach (var envcell in landblock.EnvCells)
                AddEnvCell(envcell);
        }

        public static Matrix Buildings = Matrix.CreateTranslation(Vector3.UnitZ * 0.01f);

        public void AddEnvCell(R_EnvCell envCell)
        {
            var transform = envCell.WorldTransform * Buildings;
            var env = envCell.Environment;

            foreach (var cellStruct in env.R_CellStructs.Values)
            {
                var vertices = cellStruct.VertexArray;

                foreach (var polygon in cellStruct.Polygons)
                {
                    var surfaceIdx = polygon._polygon.PosSurface;
                    var surfaceID = envCell.EnvCell._envCell.Surfaces[surfaceIdx];

                    RB_EnvCell.TryGetValue(surfaceID, out var batch);

                    if (batch == null)
                    {
                        var texture = envCell.Textures[surfaceIdx];
                        batch = new RenderBatch(texture);
                        RB_EnvCell.Add(surfaceID, batch);
                    }
                    batch.AddPolygon(vertices, polygon, transform);
                }
            }
            foreach (var staticObj in envCell.StaticObjs)
                AddStaticObj(staticObj, RB_StaticObjs);
        }

        public void BuildTerrain()
        {
            foreach (var batch in TerrainGroups.Values)
                batch.BuildBuffer();
        }

        public void BuildBuffers()
        {
            BuildTerrain();

            BuildBuffer(RB_StaticObjs);
            BuildBuffer(RB_Buildings);
            BuildBuffer(RB_EnvCell);
            BuildBuffer(RB_Scenery);

            //QueryBuffers();
        }

        public void QueryBuffers()
        {
            var terrainCnt = 0;

            foreach (var buffer in TerrainGroups.Values)
                terrainCnt += buffer.Vertices.Count;

            var staticObjCnt = QueryBuffer(RB_StaticObjs);
            var buildingCnt = QueryBuffer(RB_Buildings);
            var sceneryCnt = QueryBuffer(RB_Scenery);
            var envCellCnt = QueryBuffer(RB_EnvCell);

            Console.WriteLine($"Terrain: {terrainCnt:N0} / {TerrainGroups.Count:N0}");
            Console.WriteLine($"StaticObjs: {staticObjCnt:N0} / {RB_StaticObjs.Count:N0}");
            Console.WriteLine($"Buildings: {buildingCnt:N0} / {RB_Buildings.Count:N0}");
            Console.WriteLine($"Scenery: {sceneryCnt:N0} / {RB_Scenery.Count:N0}");
            Console.WriteLine($"EnvCells: {envCellCnt:N0} / {RB_EnvCell.Count:N0}");
        }

        public int QueryBuffer(Dictionary<uint, RenderBatch> buffer)
        {
            var count = 0;

            foreach (var b in buffer.Values)
                count += b.Vertices.Count;

            return count;
        }

        public void BuildBuffer(Dictionary<uint, RenderBatch> batches)
        {
            foreach (var batch in batches.Values)
                batch.BuildBuffer();
        }

        public static void SetRasterizerState(CullMode cullMode = CullMode.CullClockwiseFace)
        {
            var rs = new RasterizerState();

            rs.CullMode = cullMode;
            rs.FillMode = FillMode.Solid;

            GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            GraphicsDevice.RasterizerState = rs;
        }

        public void Draw()
        {
            Effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            Effect.Parameters["xLightDirection"].SetValue(-Vector3.UnitZ);
            Effect.Parameters["xAmbient"].SetValue(0.5f);

            DrawTerrain();

            DrawBuffer(RB_StaticObjs);
            DrawBuffer(RB_Buildings);
            DrawBuffer(RB_EnvCell);
            DrawBuffer(RB_Scenery);
        }

        public void DrawTerrain()
        {
            SetRasterizerState();
            Effect.CurrentTechnique = Effect.Techniques["LandscapeSinglePass"];

            foreach (var batch in TerrainGroups.Values)
                batch.Draw();
        }

        public void DrawBuffer(Dictionary<uint, RenderBatch> batches)
        {
            var cullMode = WorldViewer.Instance.DungeonMode ? CullMode.CullClockwiseFace : CullMode.None;

            SetRasterizerState(cullMode);  // todo: neg uv indices
            //Effect.CurrentTechnique = Effect.Techniques["TexturedNoShading"];
            Effect.CurrentTechnique = Effect.Techniques["Textured"];

            foreach (var batch in batches.Values)
                batch.Draw();
        }
    }
}

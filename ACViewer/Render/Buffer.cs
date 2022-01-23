using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.Server.Physics;

using ACViewer.Enum;

namespace ACViewer.Render
{
    public class Buffer
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public Dictionary<uint, TerrainBatch> TerrainGroups { get; set; }   // key: surfnum
        //public Dictionary<uint, InstanceBatch> RB_Instances { get; set; }   // key: setup id

        //public Dictionary<uint, RenderBatch> RB_EnvCell { get; set; };    // key: surface id
        public Dictionary<TextureSet, InstanceBatch> RB_EnvCell { get; set; }

        public Dictionary<uint, RenderBatch> RB_StaticObjs { get; set; }
        //public Dictionary<uint, InstanceBatch> RB_StaticObjs { get; set; }

        //public Dictionary<uint, RenderBatch> RB_Buildings { get; set; }
        public Dictionary<uint, InstanceBatch> RB_Buildings { get; set; }

        //public Dictionary<uint, RenderBatch> RB_Scenery { get; set; }
        public Dictionary<uint, InstanceBatch> RB_Scenery { get; set; }

        public ParticleBatch RB_Particles { get; set; }

        public static Effect Effect { get => Render.Effect; }

        public Buffer()
        {
            Init();
        }

        public void Init()
        {
            TerrainGroups = new Dictionary<uint, TerrainBatch>();
            //RB_Instances = new Dictionary<uint, InstanceBatch>();
            //RB_EnvCell = new Dictionary<uint, RenderBatch>();
            RB_EnvCell = new Dictionary<TextureSet, InstanceBatch>();
            RB_StaticObjs = new Dictionary<uint, RenderBatch>();
            //RB_StaticObjs = new Dictionary<uint, InstanceBatch>();
            //RB_Buildings = new Dictionary<uint, RenderBatch>();
            RB_Buildings = new Dictionary<uint, InstanceBatch>();
            //RB_Scenery = new Dictionary<uint, RenderBatch>();
            RB_Scenery = new Dictionary<uint, InstanceBatch>();
            RB_Particles = new ParticleBatch();
        }

        public void ClearBuffer()
        {
            foreach (var batch in TerrainGroups.Values)
                batch.Dispose();

            //ClearBuffer(RB_Instances);
            ClearBuffer(RB_EnvCell);
            ClearBuffer(RB_StaticObjs);
            ClearBuffer(RB_Buildings);
            ClearBuffer(RB_Scenery);
            ClearBuffer(RB_Particles);

            Init();
        }

        public void ClearBuffer(Dictionary<uint, RenderBatch> batches)
        {
            foreach (var batch in batches.Values)
                batch.Dispose();
        }

        public void ClearBuffer(Dictionary<uint, InstanceBatch> batches)
        {
            foreach (var batch in batches.Values)
                batch.Dispose();
        }

        public void ClearBuffer(Dictionary<TextureSet, InstanceBatch> batches)
        {
            foreach (var batch in batches.Values)
                batch.Dispose();
        }

        public void ClearBuffer(ParticleBatch batch)
        {
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
                //AddInstanceObj(obj, RB_StaticObjs);
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

                var frame = setup._setup.Id != 0 ? obj.PartArray.Parts[i].PhysicsPart.Pos : obj.PhysicsObj.Position;

                var transform = frame.ToXna();

                if (i < setup._setup.DefaultScale.Count)
                    transform = Matrix.CreateScale(setup._setup.DefaultScale[i].ToXna()) * transform;

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

        public void AddInstanceObj(R_PhysicsObj obj, Dictionary<uint, InstanceBatch> batches)
        {
            var setupID = obj.Setup.Setup._setup.Id;
            if (setupID == 0)
                setupID = obj.Setup.Setup.Parts[0]._gfxObj.Id;
            if (setupID == 0)
            {
                Console.WriteLine($"Couldn't find instance ID");
                return;
            }
            batches.TryGetValue(setupID, out var batch);
            if (batch == null)
            {
                batch = new InstanceBatch(obj);
                batches.Add(setupID, batch);
            }
            else
                batch.AddInstance(obj);
        }

        public void AddInstanceObj(R_EnvCell envCell, Dictionary<TextureSet, InstanceBatch> batches)
        {
            var textureSet = new TextureSet(envCell);
            batches.TryGetValue(textureSet, out var batch);
            if (batch == null)
            {
                batch = new InstanceBatch(envCell);
                batches.Add(textureSet, batch);
            }
            else
                batch.AddInstance(envCell);
        }

        public void AddBuildings(R_Landblock landblock)
        {
            foreach (var building in landblock.Buildings)
                //AddStaticObj(building, RB_Buildings);
                AddInstanceObj(building, RB_Buildings);
        }

        public void AddScenery(R_Landblock landblock)
        {
            foreach (var scenery in landblock.Scenery)
                //AddStaticObj(scenery, RB_Scenery);
                AddInstanceObj(scenery, RB_Scenery);
        }

        public void AddEnvCells(R_Landblock landblock)
        {
            foreach (var envcell in landblock.EnvCells)
            {
                //AddEnvCell(envcell);
                AddInstanceObj(envcell, RB_EnvCell);

                foreach (var staticObj in envcell.StaticObjs)
                    AddStaticObj(staticObj, RB_StaticObjs);
                    //AddInstanceObj(staticObj, RB_StaticObjs);
            }
        }

        public void AddEmitter(ParticleEmitter emitter)
        {
            var setupID = emitter.PhysicsObj.PartArray.Setup._dat.Id;

            //Console.WriteLine($"AddEmitterObj: {setupID:X8}");

            if (setupID != 0)
            {
                Console.WriteLine($"Unhandled particle emitter {emitter.Info._info.Id:X8} for GfxObj {setupID:X8}");
                return;
            }
            RB_Particles.AddEmitter(emitter);
        }

        public static readonly Matrix Buildings = Matrix.CreateTranslation(Vector3.UnitZ * 0.01f);

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

                    //RB_EnvCell.TryGetValue(surfaceID, out var batch);
                    InstanceBatch batch = null;

                    if (batch == null)
                    {
                        var texture = envCell.Textures[surfaceIdx];
                        //batch = new RenderBatch(texture);
                        //RB_EnvCell.Add(surfaceID, batch);
                    }
                    //batch.AddPolygon(vertices, polygon, transform);
                }
            }
            foreach (var staticObj in envCell.StaticObjs)
                AddStaticObj(staticObj, RB_StaticObjs);
                //AddInstanceObj(staticObj, RB_StaticObjs);
        }

        public void BuildTerrain()
        {
            foreach (var batch in TerrainGroups.Values)
                batch.BuildBuffer();
        }

        public void BuildBuffers()
        {
            BuildTerrain();

            //BuildBuffer(RB_Instances);
            BuildBuffer(RB_StaticObjs);
            BuildBuffer(RB_Buildings);
            BuildBuffer(RB_EnvCell);
            BuildBuffer(RB_Scenery);

            //QueryBuffers();

            //DebugBuffer(RB_StaticObjs);
        }

        public void DebugBuffer(Dictionary<uint, InstanceBatch> objects)
        {
            Console.WriteLine($"\nRB_StaticObjs:");
            Console.WriteLine($"Total unique setups: {objects.Count}");

            //DebugBuffer_GfxObj(objects);
            DebugBuffer_Textures(objects);
        }

        public void DebugBuffer_GfxObj(Dictionary<uint, InstanceBatch> objects)
        {
            var gfxObjs = QueryGfxObjInfo(objects);

            Console.WriteLine($"GfxObj refs: {gfxObjs.Values.Sum()}");
            Console.WriteLine($"GfxObj uniques: {gfxObjs.Count}");

            var g2s_index = GfxObjToSetup(objects);

            Console.WriteLine($"GfxObj -> Setup ({g2s_index.Count}):");
            foreach (var entry in g2s_index)
            {
                var gfxObjID = entry.Key;
                var setupIDs = entry.Value;
                Console.WriteLine($"{gfxObjID:X8} ({setupIDs.Count}): {string.Join(",", setupIDs.Select(i => i.ToString("X8")))}");
            }

            var s2g_index = SetupToGfxObj(objects);

            Console.WriteLine($"\nSetup -> GfxObjs ({s2g_index.Count}):");
            foreach (var entry in s2g_index)
            {
                var setupID = entry.Key;
                var gfxObjIDs = entry.Value;
                Console.WriteLine($"{setupID:X8} ({gfxObjIDs.Count}): {string.Join(",", gfxObjIDs.Select(i => i.ToString("X8")))}");
            }
        }

        public void DebugBuffer_Textures(Dictionary<uint, InstanceBatch> objects)
        {
            var textures = QueryTextureInfo(objects);

            Console.WriteLine($"Texture refs: {textures.Values.Sum()}");
            Console.WriteLine($"Texture uniques: {textures.Count}");

            var t2s_index = TextureToSetup(objects);

            Console.WriteLine($"Texture -> Setup ({t2s_index.Count}):");
            foreach (var entry in t2s_index)
            {
                var textureID = entry.Key;
                var setupIDs = entry.Value;
                Console.WriteLine($"{textureID:X8} ({setupIDs.Count}): {string.Join(",", setupIDs.Select(i => i.ToString("X8")))}");
            }

            var s2t_index = SetupToTexture(objects);

            Console.WriteLine($"\nSetup -> Texture ({s2t_index.Count}):");
            foreach (var entry in s2t_index)
            {
                var setupID = entry.Key;
                var textureIDs = entry.Value;
                Console.WriteLine($"{setupID:X8} ({textureIDs.Count}): {string.Join(",", textureIDs.Select(i => i.ToString("X8")))}");
            }
        }

        public Dictionary<uint, int> QueryGfxObjInfo(Dictionary<uint, InstanceBatch> objects)
        {
            var gfxObjs = new Dictionary<uint, int>();

            foreach (var obj in objects.Values)
            {
                foreach (var part in obj.R_PhysicsObj.PhysicsObj.PartArray.Parts)
                {
                    var gfxObjID = part.GfxObj.ID;

                    if (!gfxObjs.TryGetValue(gfxObjID, out var gfxObj))
                        gfxObjs.Add(gfxObjID, 1);
                    else
                        gfxObjs[gfxObjID]++;
                }
            }
            return gfxObjs;
        }

        public Dictionary<uint, int> QueryTextureInfo(Dictionary<uint, InstanceBatch> objects)
        {
            var textures = new Dictionary<uint, int>();

            foreach (var obj in objects.Values)
            {
                foreach (var part in obj.R_PhysicsObj.PhysicsObj.PartArray.Parts)
                {
                    var surfaces = part.GfxObj._dat.Surfaces;

                    foreach (var surfaceID in surfaces)
                    {
                        if (!textures.TryGetValue(surfaceID, out var textureID))
                            textures.Add(surfaceID, 1);
                        else
                            textures[surfaceID]++;
                    }
                }
            }
            return textures;
        }

        public Dictionary<uint, List<uint>> GfxObjToSetup(Dictionary<uint, InstanceBatch> objects)
        {
            var table = new Dictionary<uint, List<uint>>();

            foreach (var obj in objects.Values)
            {
                var setupID = obj.R_PhysicsObj.PhysicsObj.PartArray.Setup._dat.Id;
                if (setupID == 0)
                    setupID = obj.R_PhysicsObj.PhysicsObj.PartArray.Parts[0].GfxObj.ID;

                foreach (var part in obj.R_PhysicsObj.PhysicsObj.PartArray.Parts)
                {
                    var gfxObjID = part.GfxObj.ID;

                    if (!table.TryGetValue(gfxObjID, out var gfxObj))
                        table.Add(gfxObjID, new List<uint>() { setupID });
                    else if (!gfxObj.Contains(setupID))
                        gfxObj.Add(setupID);
                }
            }
            return table;
        }

        public Dictionary<uint, List<uint>> SetupToGfxObj(Dictionary<uint, InstanceBatch> objects)
        {
            var table = new Dictionary<uint, List<uint>>();

            foreach (var obj in objects.Values)
            {
                var setupID = obj.R_PhysicsObj.PhysicsObj.PartArray.Setup._dat.Id;
                if (setupID == 0)
                    setupID = obj.R_PhysicsObj.PhysicsObj.PartArray.Parts[0].GfxObj.ID;

                table.TryGetValue(setupID, out var gfxObjIDs);
                if (gfxObjIDs != null) continue;

                gfxObjIDs = new List<uint>();

                foreach (var part in obj.R_PhysicsObj.PhysicsObj.PartArray.Parts)
                {
                    var gfxObjID = part.GfxObj.ID;

                    if (!gfxObjIDs.Contains(gfxObjID))
                        gfxObjIDs.Add(gfxObjID);
                }
                table.Add(setupID, gfxObjIDs);
            }
            return table;
        }

        public Dictionary<uint, List<uint>> TextureToSetup(Dictionary<uint, InstanceBatch> objects)
        {
            var table = new Dictionary<uint, List<uint>>();

            foreach (var obj in objects.Values)
            {
                var setupID = obj.R_PhysicsObj.PhysicsObj.PartArray.Setup._dat.Id;
                if (setupID == 0)
                    setupID = obj.R_PhysicsObj.PhysicsObj.PartArray.Parts[0].GfxObj.ID;

                foreach (var part in obj.R_PhysicsObj.PhysicsObj.PartArray.Parts)
                {
                    foreach (var surfaceID in part.GfxObj._dat.Surfaces)
                    {
                        if (!table.TryGetValue(surfaceID, out var setup))
                            table.Add(surfaceID, new List<uint>() { setupID });
                        else if (!setup.Contains(setupID))
                            setup.Add(setupID);
                    }
                }
            }
            return table;
        }

        public Dictionary<uint, List<uint>> SetupToTexture(Dictionary<uint, InstanceBatch> objects)
        {
            var table = new Dictionary<uint, List<uint>>();

            foreach (var obj in objects.Values)
            {
                var setupID = obj.R_PhysicsObj.PhysicsObj.PartArray.Setup._dat.Id;
                if (setupID == 0)
                    setupID = obj.R_PhysicsObj.PhysicsObj.PartArray.Parts[0].GfxObj.ID;

                table.TryGetValue(setupID, out var textureIDs);
                if (textureIDs != null) continue;

                textureIDs = new List<uint>();

                foreach (var part in obj.R_PhysicsObj.PhysicsObj.PartArray.Parts)
                {
                    foreach (var textureID in part.GfxObj._dat.Surfaces)
                    {
                        if (!textureIDs.Contains(textureID))
                            textureIDs.Add(textureID);
                    }
                }
                table.Add(setupID, textureIDs);
            }
            return table;
        }

        public void QueryBuffers()
        {
            var terrainCnt = 0;

            foreach (var buffer in TerrainGroups.Values)
                terrainCnt += buffer.Vertices.Count;

            var staticObjCnt = QueryBuffer(RB_StaticObjs);
            //var staticObjCnt = QueryBuffer(RB_StaticObjs, out var sDrawCnt);
            //var buildingCnt = QueryBuffer(RB_Buildings);
            var buildingCnt = QueryBuffer(RB_Buildings, out var bDrawCnt);
            //var sceneryCnt = QueryBuffer(RB_Scenery);
            var sceneryCnt = QueryBuffer(RB_Scenery, out var nDrawCnt);
            //var envCellCnt = QueryBuffer(RB_EnvCell);
            var envCellCnt = QueryBuffer(RB_EnvCell, out var eDrawCnt);
            //var instanceCnt = QueryBuffer(RB_Instances, out var drawCnt);

            Console.WriteLine($"Terrain: {terrainCnt:N0} / {TerrainGroups.Count:N0}");
            Console.WriteLine($"StaticObjs: {staticObjCnt:N0} / {RB_StaticObjs.Count:N0}");
            //Console.WriteLine($"StaticObjs: {staticObjCnt:N0} / {RB_StaticObjs.Count:N0} / {sDrawCnt:N0}");
            //Console.WriteLine($"Buildings: {buildingCnt:N0} / {RB_Buildings.Count:N0}");
            Console.WriteLine($"Buildings: {buildingCnt:N0} / {RB_Buildings.Count:N0} / {bDrawCnt:N0}");
            //Console.WriteLine($"Scenery: {sceneryCnt:N0} / {RB_Scenery.Count:N0}");
            Console.WriteLine($"Scenery: {sceneryCnt:N0} / {RB_Scenery.Count:N0} / {nDrawCnt:N0}");
            //Console.WriteLine($"EnvCells: {envCellCnt:N0} / {RB_EnvCell.Count:N0}");
            Console.WriteLine($"EnvCells: {envCellCnt:N0} / {RB_EnvCell.Count:N0} / {eDrawCnt:N0}");
            //Console.WriteLine($"Instances: {instanceCnt:N0} / {RB_Instances.Count:N0} / {drawCnt:N0}");
            Console.WriteLine();
        }

        public int QueryBuffer(Dictionary<uint, RenderBatch> buffer)
        {
            var count = 0;

            foreach (var b in buffer.Values)
                count += b.Vertices.Count;

            return count;
        }

        public int QueryBuffer(Dictionary<uint, InstanceBatch> buffer, out int drawCnt)
        {
            var vertexCnt = 0;
            drawCnt = 0;

            foreach (var batch in buffer.Values)
            {
                foreach (var draw in batch.DrawCalls.Values)
                    vertexCnt += draw.Vertices.Count;

                drawCnt += batch.DrawCalls.Count;
            }
            return vertexCnt;
        }

        public int QueryBuffer(Dictionary<TextureSet, InstanceBatch> buffer, out int drawCnt)
        {
            var vertexCnt = 0;
            drawCnt = 0;

            foreach (var batch in buffer.Values)
            {
                foreach (var draw in batch.DrawCalls.Values)
                    vertexCnt += draw.Vertices.Count;

                drawCnt += batch.DrawCalls.Count;
            }
            return vertexCnt;
        }

        public void BuildBuffer(Dictionary<uint, RenderBatch> batches)
        {
            foreach (var batch in batches.Values)
                batch.BuildBuffer();
        }

        public void BuildBuffer(Dictionary<uint, InstanceBatch> batches)
        {
            foreach (var batch in batches.Values)
                batch.OnCompleted();
        }

        public void BuildBuffer(Dictionary<TextureSet, InstanceBatch> batches)
        {
            foreach (var batch in batches.Values)
                batch.OnCompleted();
        }

        public void BuildParticleBuffer()
        {
            RB_Particles.OnCompleted();
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

            PerfTimer.Start(ProfilerSection.Draw);
            
            DrawTerrain();

            DrawBuffer(RB_StaticObjs);
            DrawBuffer(RB_Buildings);
            DrawBuffer(RB_EnvCell, true);
            DrawBuffer(RB_Scenery);
            //DrawBuffer(RB_Instances);

            PerfTimer.Stop(ProfilerSection.Draw);
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

        public void DrawBuffer(Dictionary<uint, InstanceBatch> batches)
        {
            var cullMode = WorldViewer.Instance.DungeonMode ? CullMode.CullClockwiseFace : CullMode.None;

            SetRasterizerState(cullMode);  // todo: neg uv indices
            //Effect.CurrentTechnique = Effect.Techniques["TexturedInstanceNoShading"];
            Effect.CurrentTechnique = Effect.Techniques["TexturedInstance"];

            foreach (var batch in batches.Values)
                batch.Draw();
        }

        public void DrawBuffer(Dictionary<TextureSet, InstanceBatch> batches, bool culling = false)
        {
            var cullMode = WorldViewer.Instance.DungeonMode || culling ? CullMode.CullClockwiseFace : CullMode.None;

            SetRasterizerState(cullMode);  // todo: neg uv indices
            //Effect.CurrentTechnique = Effect.Techniques["TexturedInstanceNoShading"];
            Effect.CurrentTechnique = Effect.Techniques["TexturedInstanceEnv"];

            foreach (var batch in batches.Values)
                batch.Draw();
        }

        public void UpdateParticles()
        {
            RB_Particles.UpdateBuffers();
        }

        public void DrawParticles()
        {
            SetRasterizerState(CullMode.None);

            Effect.CurrentTechnique = Effect.Techniques["ParticleInstance"];

            RB_Particles.Draw();
        }
    }
}

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.DatLoader.Entity;
using ACE.Server.Physics;

using ACViewer.Enum;
using ACViewer.Model;

namespace ACViewer.Render
{
    public class Buffer
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public Dictionary<TextureFormat, TextureAtlasChain> TextureAtlasChains { get; set; }

        public TerrainBatch TerrainBatch { get; set; }

        public Dictionary<TextureSet, InstanceBatch> RB_EnvCell { get; set; }
        public Dictionary<uint, GfxObjInstance_Shared> RB_StaticObjs { get; set; }
        public Dictionary<uint, GfxObjInstance_Shared> RB_Buildings { get; set; }
        public Dictionary<uint, GfxObjInstance_Shared> RB_Scenery { get; set; }

        public ParticleBatch RB_Particles { get; set; }

        public Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared> RB_Instances { get; set; }
        public Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared> RB_Encounters { get; set; }

        public Dictionary<TextureFormat, TextureAtlasChain> InstanceTextureAtlasChains { get; set; }

        public Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared> RB_Animated { get; set; }

        public Dictionary<TextureFormat, TextureAtlasChain> AnimatedTextureAtlasChains { get; set; }    // is this really needed?

        public static Effect Effect { get => Render.Effect; }

        public static Effect Effect_Clamp { get => Render.Effect_Clamp; }

        public static bool drawTerrain { get; set; } = true;
        public static bool drawEnvCells { get; set; } = true;
        public static bool drawStaticObjs { get; set; } = true;
        public static bool drawBuildings { get; set; } = true;
        public static bool drawScenery { get; set; } = true;
        public static bool drawAlpha { get; set; } = true;
        public static bool drawInstances { get; set; } = true;
        public static bool drawEncounters { get; set; } = true;

        public Buffer()
        {
            Init();
        }

        public void Init()
        {
            TextureAtlasChains = new Dictionary<TextureFormat, TextureAtlasChain>();

            var overlayFormat = new TextureFormat(SurfaceFormat.Color, 512, 512, false);
            var alphaFormat = new TextureFormat(SurfaceFormat.Alpha8, 512, 512, false);

            var overlayAtlasChain = new TextureAtlasChain(overlayFormat);
            var alphaAtlasChain = new TextureAtlasChain(alphaFormat);

            TextureAtlasChains.Add(overlayFormat, overlayAtlasChain);
            TextureAtlasChains.Add(alphaFormat, alphaAtlasChain);

            TerrainBatch = new TerrainBatch(overlayAtlasChain, alphaAtlasChain);

            RB_EnvCell = new Dictionary<TextureSet, InstanceBatch>();

            RB_StaticObjs = new Dictionary<uint, GfxObjInstance_Shared>();
            RB_Buildings = new Dictionary<uint, GfxObjInstance_Shared>();
            RB_Scenery = new Dictionary<uint, GfxObjInstance_Shared>();

            RB_Particles = new ParticleBatch();

            RB_Instances = new Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared>();
            RB_Encounters = new Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared>();

            RB_Animated = new Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared>();

            InstanceTextureAtlasChains = new Dictionary<TextureFormat, TextureAtlasChain>();

            AnimatedTextureAtlasChains = new Dictionary<TextureFormat, TextureAtlasChain>();
        }

        public void ClearBuffer()
        {
            TerrainBatch.Dispose();

            ClearBuffer(RB_EnvCell);

            ClearBuffer(RB_StaticObjs);
            ClearBuffer(RB_Buildings);
            ClearBuffer(RB_Scenery);

            ClearBuffer(RB_Particles);

            ClearBuffer(RB_Instances);
            ClearBuffer(RB_Encounters);

            ClearBuffer(RB_Animated);

            foreach (var textureAtlasChain in TextureAtlasChains.Values)
                textureAtlasChain.Dispose();

            foreach (var textureAtlasChain in InstanceTextureAtlasChains.Values)
                textureAtlasChain.Dispose();

            foreach (var textureAtlasChain in AnimatedTextureAtlasChains.Values)
                textureAtlasChain.Dispose();

            Init();
        }

        public void ClearBuffer(Dictionary<TextureSet, InstanceBatch> batches)
        {
            foreach (var batch in batches.Values)
                batch.Dispose();
        }

        public void ClearBuffer(Dictionary<uint, GfxObjInstance_Shared> batches)
        {
            foreach (var batch in batches.Values)
                batch.Dispose();
        }

        public void ClearBuffer(Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared> batches)
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
            TerrainBatch.AddTerrain(landblock);
        }

        public void AddStaticObjs(R_Landblock landblock)
        {
            foreach (var obj in landblock.StaticObjs)
                AddInstanceObj(obj, RB_StaticObjs);
        }

        public void AddInstanceObj(R_PhysicsObj obj, Dictionary<uint, GfxObjInstance_Shared> batches)
        {
            foreach (var part in obj.PartArray.Parts)
            {
                var gfxObjId = part.R_GfxObj.GfxObj.ID;

                if (!batches.TryGetValue(gfxObjId, out var batch))
                {
                    var _gfxObj = GfxObjCache.Get(gfxObjId);

                    batch = new GfxObjInstance_Shared(_gfxObj, TextureAtlasChains);
                    batches.Add(gfxObjId, batch);
                }

                var position = part.PhysicsPart.Pos.GetWorldPos();
                var orientation = part.PhysicsPart.Pos.Frame.Orientation.ToXna();
                var scale = part.PhysicsPart.GfxObjScale.ToXna();

                batch.AddInstance(position, orientation, scale);
            }
        }

        public void AddInstanceObj(R_PhysicsObj obj, Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared> batches, Model.ObjDesc objDesc = null, Dictionary<TextureFormat, TextureAtlasChain> textureAtlasChains = null)
        {
            textureAtlasChains ??= TextureAtlasChains;

            for (var i = 0; i < obj.PartArray.Parts.Count; i++)
            {
                var part = obj.PartArray.Parts[i];

                var gfxObjId = part.R_GfxObj.GfxObj.ID;

                Dictionary<uint, uint> textureChanges = null;
                PaletteChanges paletteChanges = objDesc?.PaletteChanges;

                if (objDesc?.PartChanges != null && objDesc.PartChanges.TryGetValue((uint)part.PhysicsPart.PhysObjIndex, out var partChange))
                {
                    gfxObjId = partChange.NewGfxObjId;
                    textureChanges = partChange.TextureChanges;
                }

                var gfxObjTexturePalette = new GfxObjTexturePalette(gfxObjId, textureChanges, paletteChanges);

                if (!batches.TryGetValue(gfxObjTexturePalette, out var batch))
                {
                    var _gfxObj = GfxObjCache.Get(gfxObjId);

                    batch = new GfxObjInstance_Shared(_gfxObj, textureAtlasChains, textureChanges, paletteChanges);
                    batches.Add(gfxObjTexturePalette, batch);
                }

                var position = part.PhysicsPart.Pos.GetWorldPos();
                var orientation = part.PhysicsPart.Pos.Frame.Orientation.ToXna();
                var scale = part.PhysicsPart.GfxObjScale.ToXna();

                batch.AddInstance(position, orientation, scale);

                part.PhysicsPart.Buffer = batch;
                part.PhysicsPart.BufferIdx = batch.Instances.Count - 1;
            }
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
                AddInstanceObj(building, RB_Buildings);
        }

        public void AddScenery(R_Landblock landblock)
        {
            foreach (var scenery in landblock.Scenery)
                AddInstanceObj(scenery, RB_Scenery);
        }

        public void AddEnvCells(R_Landblock landblock)
        {
            foreach (var envcell in landblock.EnvCells)
            {
                AddInstanceObj(envcell, RB_EnvCell);

                foreach (var staticObj in envcell.StaticObjs)
                    AddInstanceObj(staticObj, RB_StaticObjs);
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

        public void AddInstance(R_PhysicsObj instance, Model.ObjDesc objDesc = null)
        {
            AddInstanceObj(instance, RB_Instances, objDesc, InstanceTextureAtlasChains);
        }

        public void AddEncounter(R_PhysicsObj encounter, Model.ObjDesc objDesc = null)
        {
            AddInstanceObj(encounter, RB_Encounters, objDesc);
        }

        public void AddPlayer(R_PhysicsObj player, Model.ObjDesc objDesc = null)
        {
            AddInstanceObj(player, RB_Animated, objDesc, AnimatedTextureAtlasChains);
        }

        public void BuildTerrain()
        {
            TerrainBatch.OnCompleted();
        }

        public void BuildBuffers()
        {
            BuildTextureAtlases();

            BuildTerrain();

            BuildBuffer(RB_EnvCell);

            BuildBuffer(RB_StaticObjs);
            BuildBuffer(RB_Buildings);
            BuildBuffer(RB_Scenery);

            QueryBuffers();
        }

        public void BuildTextureAtlases(Dictionary<TextureFormat, TextureAtlasChain> textureAtlasChains = null)
        {
            textureAtlasChains ??= TextureAtlasChains;
            
            foreach (var textureAtlas in textureAtlasChains.Values)
                textureAtlas.OnCompleted();
        }

        public void QueryBuffers()
        {
            var terrainCnt = 0;

            foreach (var tb in TerrainBatch.Batches)
                terrainCnt += tb.Vertices.Count;

            var staticObjCnt = QueryBuffer(RB_StaticObjs);
            var buildingCnt = QueryBuffer(RB_Buildings);
            var sceneryCnt = QueryBuffer(RB_Scenery);
            var envCellCnt = QueryBuffer(RB_EnvCell, out var eDrawCnt);

            Console.WriteLine($"Terrain: {terrainCnt:N0} / {(terrainCnt / 6):N0} / 1");
            Console.WriteLine($"StaticObjs: {staticObjCnt}");
            Console.WriteLine($"Buildings: {buildingCnt}");
            Console.WriteLine($"Scenery: {sceneryCnt}");
            Console.WriteLine($"EnvCells: {envCellCnt:N0} / {RB_EnvCell.Count:N0} / {eDrawCnt:N0}");
            Console.WriteLine();
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

        public InstanceBufferStats QueryBuffer(Dictionary<uint, GfxObjInstance_Shared> buffer)
        {
            var stats = new InstanceBufferStats();

            foreach (var batch in buffer.Values)
            {
                // total vertices if uninstanced / total vertices using instancing / total unique gfxobjs / total draw calls
                stats.TotalUninstancedVertices += batch.Vertices.Count * batch.Instances.Count;
                stats.TotalVerticesWithInstancing += batch.Vertices.Count + batch.Instances.Count;
                stats.TotalDrawCalls += batch.BaseFormats_Solid.Count + batch.BaseFormats_Alpha.Count;
                stats.TotalUniqueGfxObjIDs++;
            }
            return stats;
        }

        public void BuildBuffer(Dictionary<TextureSet, InstanceBatch> batches)
        {
            foreach (var batch in batches.Values)
                batch.OnCompleted();
        }

        public void BuildBuffer(Dictionary<uint, GfxObjInstance_Shared> batches)
        {
            foreach (var batch in batches.Values)
                batch.OnCompleted();
        }

        public void BuildBuffer(Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared> batches)
        {
            foreach (var batch in batches.Values)
                batch.OnCompleted();
        }

        public void BuildParticleBuffer()
        {
            RB_Particles.OnCompleted();
        }

        public void UpdateParticles()
        {
            RB_Particles.UpdateBuffers();
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

            Effect_Clamp.Parameters["xWorld"].SetValue(Matrix.Identity);
            Effect_Clamp.Parameters["xLightDirection"].SetValue(-Vector3.UnitZ);
            Effect_Clamp.Parameters["xAmbient"].SetValue(0.5f);

            PerfTimer.Start(ProfilerSection.Draw);

            if (drawTerrain)
                DrawTerrain();

            if (drawEnvCells)
                DrawBuffer(RB_EnvCell, true);

            if (drawStaticObjs)
                DrawBuffer(RB_StaticObjs);

            if (drawBuildings)
                DrawBuffer(RB_Buildings);

            if (drawScenery)
                DrawBuffer(RB_Scenery);

            if (drawInstances && Server.InstancesLoaded)
                DrawBuffer(RB_Instances);

            if (drawEncounters && Server.EncountersLoaded)
                DrawBuffer(RB_Encounters);

            DrawBuffer(RB_Animated);

            if (Picker.HitVertices != null)
                Picker.DrawHitPoly();

            PerfTimer.Stop(ProfilerSection.Draw);
        }

        public void DrawTerrain()
        {
            SetRasterizerState();

            TerrainBatch.Draw();
        }

        public void DrawBuffer(Dictionary<uint, GfxObjInstance_Shared> batches)
        {
            SetRasterizerState(CullMode.None);

            foreach (var batch in batches.Values)
                batch.Draw();
        }

        public void DrawBuffer(Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared> batches)
        {
            SetRasterizerState(CullMode.None);

            foreach (var batch in batches.Values)
                batch.Draw();
        }

        public void DrawBuffer(Dictionary<TextureSet, InstanceBatch> batches, bool culling = false)
        {
            var cullMode = WorldViewer.Instance.DungeonMode || culling ? CullMode.CullClockwiseFace : CullMode.None;

            SetRasterizerState(cullMode);  // todo: neg uv indices

            Effect.CurrentTechnique = Effect.Techniques["TexturedInstanceEnv"];

            Effect_Clamp.CurrentTechnique = Effect_Clamp.Techniques["TexturedInstanceEnv"];

            foreach (var batch in batches.Values)
                batch.Draw();
        }

        public void DrawParticles()
        {
            SetRasterizerState(CullMode.None);

            Effect.CurrentTechnique = Effect.Techniques["ParticleInstance"];

            RB_Particles.Draw();
        }
    }
}

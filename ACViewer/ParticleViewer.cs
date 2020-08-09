using System.Collections.Generic;
using System.Linq;
using ACE.DatLoader;
using ACE.DatLoader.Entity.AnimationHooks;
using ACE.Entity.Enum;
using ACE.Server.Physics;
using ACE.Server.Physics.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop.Input;
using ACViewer.Model;
using ACViewer.Render;
using ACViewer.View;

namespace ACViewer
{
    public class ParticleViewer
    {
        public static MainWindow MainWindow => MainWindow.Instance;
        public static GameView GameView => GameView.Instance;

        public static Player Player => GameView.Player;

        public static ParticleViewer Instance;
        public static Camera Camera => GameView.Camera;
        public static WpfKeyboard Keyboard => GameView._keyboard;
        public static KeyboardState PrevKeyboardState;

        public static GraphicsDevice GraphicsDevice => GameView.GraphicsDevice;
        public static Effect Effect => Render.Render.Effect;


        public ParticleViewer()
        {
            Instance = this;
        }

        public float GetModIdx(List<float> mods, float mod)
        {
            foreach (var m in mods)
            {
                if (mod >= m)
                    return m;
            }
            return mods[0];
        }

        public List<uint> GetEmitterInfoIDs(uint pEffectTableID, PlayScript playScript, float mod = 0.0f)
        {
            var pEffectTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.PhysicsScriptTable>(pEffectTableID);
            var scripts = pEffectTable.ScriptTable[(uint)playScript].Scripts;

            var modIdx = GetModIdx(scripts.Select(s => s.Mod).OrderByDescending(m => m).ToList(), mod);

            var scriptModDataEntry = scripts.Where(s => s.Mod == modIdx).FirstOrDefault();

            return GetEmitterInfoIDs(scriptModDataEntry.ScriptId);
        }

        public List<uint> GetEmitterInfoIDs(uint scriptID, float mod = 0.0f)
        {
            var emitterInfoIDs = new List<uint>();

            var script = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.PhysicsScript>(scriptID);

            foreach (var scriptDataEntry in script.ScriptData)
            {
                // AnimationHook
                if (scriptDataEntry.Hook.HookType == AnimationHookType.CreateParticle)
                {
                    var particleHook = scriptDataEntry.Hook as CreateParticleHook;
                    var emitterInfoID = particleHook.EmitterInfoId;
                    emitterInfoIDs.Add(emitterInfoID);
                }
            }
            return emitterInfoIDs;
        }

        public void InitEmitter(List<uint> emitterInfoIDs, float mod = 0.0f)
        {
            GameView.ViewMode = ViewMode.Particle;

            foreach (var emitterInfoID in emitterInfoIDs)
            {
                var emitterInfo = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.ParticleEmitterInfo>(emitterInfoID);
                MainWindow.Status.WriteLine($"ParticleEmitterInfo.ID: {emitterInfoID:X8}");

                var frame = new AFrame();
                Player.PhysicsObj.create_particle_emitter(emitterInfoID, 0, frame, 0);
            }
        }

        public void InitEmitter(uint scriptID, float mod = 0.0f)
        {
            if (scriptID >> 24 == 0x32)
            {
                InitEmitter(new List<uint>() { scriptID }, mod);
                return;
            }

            var emitterInfoIDs = GetEmitterInfoIDs(scriptID, mod);
            InitEmitter(emitterInfoIDs, mod);
        }

        public void InitEmitter(uint pEffectTableID, PlayScript playScript, float mod = 0.0f)
        {
            var emitterInfoIDs = GetEmitterInfoIDs(pEffectTableID, playScript, mod);
            InitEmitter(emitterInfoIDs, mod);
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.C) && !PrevKeyboardState.IsKeyDown(Keys.C))
            {
                // cancel all emitters in progress
                Player.PhysicsObj.ParticleManager.ParticleTable.Clear();
            }

            Player.Update(gameTime);
            Camera.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0, 0, 0));

            Effect.CurrentTechnique = Effect.Techniques["ColoredNoShading"];
            Effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            Effect.Parameters["xView"].SetValue(Camera.ViewMatrix);
            Effect.Parameters["xProjection"].SetValue(Camera.ProjectionMatrix);

            DrawParticles();
        }

        public void DrawParticles()
        {
            var rs = new RasterizerState();
            rs.CullMode = Microsoft.Xna.Framework.Graphics.CullMode.CullClockwiseFace;
            //rs.CullMode = Microsoft.Xna.Framework.Graphics.CullMode.None;
            rs.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rs;

            var particleManager = Player.PhysicsObj.ParticleManager;

            if (particleManager == null || particleManager.ParticleTable.Count == 0)
                return;

            foreach (var emitter in particleManager.ParticleTable.Values.Where(p => p != null))
            {
                foreach (var part in emitter.Parts.Where(p => p != null))
                {
                    var gfxObjID = part.GfxObj.ID;
                    var gfxObj = GfxObjCache.Get(gfxObjID);
                    var texture = gfxObj.Textures[0];

                    bool isPointSprite = gfxObj._gfxObj.SortCenter.NearZero() && gfxObj._gfxObj.Id != 0x0100283B;

                    if (isPointSprite)
                        DrawPointSprite(gfxObj, part, texture);
                    else
                        DrawGfxObj(gfxObj, part, texture);
                }
            }
        }

        public void DrawPointSprite(GfxObj gfxObj, PhysicsPart part, Texture2D texture)
        {
            GraphicsDevice.SetVertexBuffer(Billboard.VertexBuffer);
            GraphicsDevice.Indices = Billboard.IndexBuffer;

            var translateWorld = Matrix.CreateTranslation(part.Pos.Frame.Origin.ToXna()) * Matrix.CreateFromQuaternion(part.Pos.Frame.Orientation.ToXna());

            Effect.CurrentTechnique = Effect.Techniques["PointSprite"];
            Effect.Parameters["xWorld"].SetValue(translateWorld);
            Effect.Parameters["xTextures"].SetValue(texture);
            Effect.Parameters["xCamPos"].SetValue(Camera.Position);
            Effect.Parameters["xCamUp"].SetValue(Camera.Up);
            Effect.Parameters["xPointSpriteSizeX"].SetValue(part.GfxObjScale.X);
            Effect.Parameters["xPointSpriteSizeY"].SetValue(part.GfxObjScale.Y);
            Effect.Parameters["xOpacity"].SetValue(1.0f - part.CurTranslucency);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                if (gfxObj.Surfaces[0].Type.HasFlag(SurfaceType.Additive))
                    GraphicsDevice.BlendState = BlendState.Additive;
                else
                    GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                pass.Apply();

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2);
            }
        }

        public void DrawGfxObj(GfxObj gfxObj, PhysicsPart part, Texture2D texture)
        {
            GraphicsDevice.SetVertexBuffer(gfxObj.VertexBuffer);

            var translateWorld = Matrix.CreateScale(part.GfxObjScale.ToXna()) * Matrix.CreateTranslation(part.Pos.Frame.Origin.ToXna()) * Matrix.CreateFromQuaternion(part.Pos.Frame.Orientation.ToXna());

            Effect.CurrentTechnique = Effect.Techniques["TexturedNoShading"];
            Effect.Parameters["xWorld"].SetValue(translateWorld);
            Effect.Parameters["xOpacity"].SetValue(1.0f - part.CurTranslucency);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                foreach (var poly in gfxObj.Polygons)
                {
                    if (gfxObj.Surfaces[0].Type.HasFlag(SurfaceType.Additive))
                        GraphicsDevice.BlendState = BlendState.Additive;
                    else
                        GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                    //GraphicsDevice.BlendState = BlendState.AlphaBlend;

                    //GraphicsDevice.Indices = poly.IndexBuffer;
                    Effect.Parameters["xTextures"].SetValue(poly.Texture);
                    pass.Apply();

                    //var indexCnt = poly.IndexArray.Count;
                    //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexCnt / 3);
                    if (poly == null || poly._polygon == null || poly._polygon.Vertices == null)
                        continue;

                    var vertexCnt = poly._polygon.Vertices.Count;
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexCnt / 3);
                }
            }
        }
    }
}

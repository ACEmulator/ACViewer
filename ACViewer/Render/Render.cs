using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ACE.Server.Physics;
using ACE.Server.Physics.Animation;
using ACE.Server.Physics.Common;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACViewer.Config;
using ACViewer.Enum;
using ACViewer.View;

namespace ACViewer.Render
{
    public class Render
    {
        public GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public static Effect Effect { get; set; }

        // multiple SamplerStates in the same .fx file apparently don't work
        public static Effect Effect_Clamp { get; set; }

        public Camera Camera
        {
            get => GameView.Camera;
            set => GameView.Camera = value;
        }

        public Buffer Buffer { get; set; }

        public Render()
        {
            Init();
        }

        public void Init()
        {
            Effect = new Effect(GraphicsDevice, File.ReadAllBytes("Content/texture.mgfxo"));
            Effect_Clamp = new Effect(GraphicsDevice, File.ReadAllBytes("Content/texture_clamp.mgfxo"));

            if (Camera == null)
                Camera = new Camera(GameView.Instance);

            Effect.Parameters["xProjection"].SetValue(Camera.ProjectionMatrix);
            Effect_Clamp.Parameters["xProjection"].SetValue(Camera.ProjectionMatrix);

            Buffer = new Buffer();
        }

        public void SetRasterizerState(bool wireframe = true)
        {
            var rs = new RasterizerState();

            //rs.CullMode = CullMode.CullClockwiseFace;
            rs.CullMode = CullMode.None;

            if (wireframe)
                rs.FillMode = FillMode.WireFrame;
            else
                rs.FillMode = FillMode.Solid;

            GraphicsDevice.RasterizerState = rs;
        }

        public void Draw()
        {
            GraphicsDevice.Clear(ConfigManager.Config.BackgroundColors.WorldViewer);

            SetRasterizerState(false);
            
            Effect.Parameters["xView"].SetValue(Camera.ViewMatrix);
            Effect_Clamp.Parameters["xView"].SetValue(Camera.ViewMatrix);

            //landblock.Draw();
            Buffer.Draw();

            //DrawEmitters_Naive();
            DrawEmitters_Batch();
        }

        public bool ParticlesInitted { get; set; }

        public List<PhysicsObj> EmitterParentObjs { get; set; }

        public void InitEmitters()
        {
            ParticlesInitted = false;
            EmitterParentObjs = new List<PhysicsObj>();
            
            if (!MainMenu.ShowParticles) return;

            foreach (var landblock in LScape.Landblocks.Values)
            {
                foreach (var cell in landblock.LandCells.Values)
                {
                    foreach (var staticObj in cell.ObjectList.ToList())
                    {
                        var setup = staticObj.PartArray.Setup._dat;

                        // handle DefaultScriptTable also?
                        if (setup.DefaultScript != 0)
                        {
                            //Console.WriteLine($"Creating emitter for {setup.Id:X8} in {staticObj.Position.ObjCellID:X8}");
                            var createParticleHooks = ParticleViewer.Instance.GetCreateParticleHooks(setup.DefaultScript, 1.0f);

                            if (createParticleHooks.Count == 0) continue;

                            EmitterParentObjs.Add(staticObj);

                            staticObj.destroy_particle_manager();

                            foreach (var createParticleHook in createParticleHooks)
                            {
                                var emitterIdx = staticObj.create_particle_emitter(createParticleHook.EmitterInfoId, (int)createParticleHook.PartIndex, new AFrame(createParticleHook.Offset), (int)createParticleHook.EmitterId);

                                if (!staticObj.ParticleManager.ParticleTable.TryGetValue(emitterIdx, out var emitter))
                                {
                                    // can happen if HwGfxObjId==0, skip
                                    continue;
                                }
                                Buffer.AddEmitter(emitter);
                            }
                        }
                    }
                }
            }

            Buffer.BuildParticleBuffer();
            ParticlesInitted = true;

            //Console.WriteLine($"Initted {EmitterObjs.Count:N0} emitter obs w/ {Emitters:N0} emitters");
        }

        public void UpdateEmitters()
        {
            if (!MainMenu.ShowParticles || !ParticlesInitted) return;

            if (!GameView.Instance.IsActive) return;

            PerfTimer.Start(ProfilerSection.ParticleUpdate);

            //foreach (var emitterObj in EmitterParentObjs)
                //emitterObj.ParticleManager.UpdateParticles();
            
            Parallel.ForEach(EmitterParentObjs, emitterObj =>
            {
                emitterObj.ParticleManager.UpdateParticles();
            });

            Buffer.UpdateParticles();

            PerfTimer.Stop(ProfilerSection.ParticleUpdate);
        }

        public void DrawEmitters_Batch()
        {
            if (!MainMenu.ShowParticles || !ParticlesInitted) return;

            Effect.Parameters["xCamPos"].SetValue(Camera.Position);
            Effect.Parameters["xCamUp"].SetValue(Camera.Up);

            PerfTimer.Start(ProfilerSection.ParticleDraw);

            Buffer.DrawParticles();

            PerfTimer.Stop(ProfilerSection.ParticleDraw);
        }

        public void DestroyEmitters()
        {
            foreach (var emitterObj in EmitterParentObjs)
                emitterObj.destroy_particle_manager();

            ParticlesInitted = false;
        }

        // text rendering
        public SpriteBatch SpriteBatch => GameView.Instance.SpriteBatch;
        public SpriteFont Font => GameView.Instance.Font;

        private static readonly Vector2 TextPos = new Vector2(10, 10);

        public void DrawHUD()
        {
            var cameraPos = GameView.Camera.GetPosition();

            if (cameraPos != null)
            {
                SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp);
                SpriteBatch.DrawString(Font, $"Location: {cameraPos}", TextPos, Color.White);
                SpriteBatch.End();
            }
        }
    }
}

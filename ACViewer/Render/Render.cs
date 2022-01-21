using System;
using System.IO;
using System.Linq;

using ACE.Entity.Enum;

using ACE.Server.Physics;
using ACE.Server.Physics.Animation;
using ACE.Server.Physics.Common;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACViewer.Model;
using ACViewer.View;

namespace ACViewer.Render
{
    public class Render
    {
        public GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public static Effect Effect { get; set; }

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

            if (Camera == null)
                Camera = new Camera(GameView.Instance);

            Effect.Parameters["xProjection"].SetValue(Camera.ProjectionMatrix);

            Buffer = new Buffer();
        }

        public void SetRasterizerState(bool wireframe = true)
        {
            var rs = new RasterizerState();

            //rs.CullMode = CullMode.CullClockwiseFace;
            rs.CullMode = Microsoft.Xna.Framework.Graphics.CullMode.None;

            if (wireframe)
                rs.FillMode = FillMode.WireFrame;
            else
                rs.FillMode = FillMode.Solid;

            GraphicsDevice.RasterizerState = rs;
        }

        private static readonly Color BackgroundColor = new Color(32, 32, 32);

        public void Draw()
        {
            GraphicsDevice.Clear(BackgroundColor);
            SetRasterizerState(false);
            Effect.Parameters["xView"].SetValue(Camera.ViewMatrix);

            //landblock.Draw();
            Buffer.Draw();

            DrawEmitters();
        }

        public bool ParticlesInitted;

        public void InitEmitters()
        {
            ParticlesInitted = false;
            
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

                            staticObj.destroy_particle_manager();

                            foreach (var createParticleHook in createParticleHooks)
                                staticObj.create_particle_emitter(createParticleHook.EmitterInfoId, (int)createParticleHook.PartIndex, new AFrame(createParticleHook.Offset), (int)createParticleHook.EmitterId);
                        }
                    }
                }
            }
            ParticlesInitted = true;
        }

        public void UpdateEmitters()
        {
            if (!MainMenu.ShowParticles) return;

            if (!GameView.Instance.IsActive) return;
            
            foreach (var landblock in LScape.Landblocks.Values)
            {
                foreach (var cell in landblock.LandCells.Values)
                {
                    foreach (var staticObj in cell.ObjectList.ToList())
                    {
                        if (staticObj.ParticleManager != null)
                            staticObj.ParticleManager.UpdateParticles();
                    }
                }
            }
        }

        public void DrawEmitters()
        {
            if (!MainMenu.ShowParticles) return;

            foreach (var landblock in LScape.Landblocks.Values)
            {
                foreach (var cell in landblock.LandCells.Values)
                {
                    foreach (var staticObj in cell.ObjectList.ToList())
                    {
                        var num_emitters = staticObj.get_num_emitters();

                        if (num_emitters <= 0) continue;

                        foreach (var emitter in staticObj.ParticleManager.ParticleTable.Values)
                        {
                            if (emitter == null) continue;

                            foreach (var part in emitter.Parts)
                            {
                                if (part == null) continue;

                                var gfxObjID = part.GfxObj.ID;
                                var gfxObj = GfxObjCache.Get(gfxObjID);
                                var texture = gfxObj.Textures[0];

                                bool isPointSprite = gfxObj._gfxObj.SortCenter.NearZero() && gfxObj._gfxObj.Id != 0x0100283B;

                                if (isPointSprite)
                                    DrawParticle_PointSprite(gfxObj, part, texture);
                                else
                                    DrawParticle_GfxObj(gfxObj, part, texture);
                            }
                        }
                    }
                }
            }
        }

        private static readonly float pointSpriteSize = 1.8f;   // guessing

        public void DrawParticle_PointSprite(GfxObj gfxObj, PhysicsPart part, Texture2D texture)
        {
            //Console.WriteLine($"DrawParticle_PointSprite");
            
            GraphicsDevice.SetVertexBuffer(Billboard.VertexBuffer);
            GraphicsDevice.Indices = Billboard.IndexBuffer;

            var translateWorld = part.Pos.ToXna();
            
            // get initial scale from gfxobj vertices
            if (gfxObj.BoundingBox == null)
                gfxObj.BuildBoundingBox();

            Effect.CurrentTechnique = Effect.Techniques["PointSprite"];
            Effect.Parameters["xWorld"].SetValue(translateWorld);
            Effect.Parameters["xTextures"].SetValue(texture);
            Effect.Parameters["xCamPos"].SetValue(Camera.Position);
            Effect.Parameters["xCamUp"].SetValue(Camera.Up);
            Effect.Parameters["xPointSpriteSizeX"].SetValue(part.GfxObjScale.X * gfxObj.BoundingBox.MaxSize * pointSpriteSize);
            Effect.Parameters["xPointSpriteSizeY"].SetValue(part.GfxObjScale.Y * gfxObj.BoundingBox.MaxSize * pointSpriteSize);
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

        public void DrawParticle_GfxObj(GfxObj gfxObj, PhysicsPart part, Texture2D texture)
        {
            //Console.WriteLine($"DrawParticle_GfxObj");
            
            if (gfxObj.VertexBuffer == null)
                gfxObj.BuildVertexBuffer();

            GraphicsDevice.SetVertexBuffer(gfxObj.VertexBuffer);

            var translateWorld = Matrix.CreateScale(part.GfxObjScale.ToXna()) * part.Pos.ToXna();

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

                    if (poly.IndexBuffer == null)
                        poly.BuildIndexBuffer();

                    GraphicsDevice.Indices = poly.IndexBuffer;
                    Effect.Parameters["xTextures"].SetValue(poly.Texture);
                    pass.Apply();

                    var indexCnt = poly.Indices.Count;
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexCnt / 3);

                    /*if (poly._polygon.Vertices == null)
                        poly._polygon.LoadVertices(gfxObj._gfxObj.VertexArray);

                    var vertexCnt = poly._polygon.Vertices.Count;
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexCnt / 3);*/
                }
            }
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

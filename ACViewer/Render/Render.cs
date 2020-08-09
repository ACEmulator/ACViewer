using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.Entity.Enum;
using ACE.Server.Physics;
using ACViewer.Model;

namespace ACViewer.Render
{
    public class Render
    {
        public GameView GameView { get => GameView.Instance; }
        public GraphicsDevice GraphicsDevice { get => GameView.GraphicsDevice; }
        public static Effect Effect;

        public Camera Camera { get => GameView.Camera; set => GameView.Camera = value; }

        public Buffer Buffer;

        // text rendering
        public SpriteBatch SpriteBatch => GameView.SpriteBatch;

        public SpriteFont Font;

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

            Font = GameView.Content.Load<SpriteFont>("Fonts/Consolas");
        }

        public void Draw()
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

            var particleManager = GameView.Instance.Player.PhysicsObj.ParticleManager;

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
                DrawCount.NumPointSprite++;
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

                    var indexCnt = poly.Indices.Count;
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexCnt / 3);
                    DrawCount.NumGfxObj++;
                }
            }
        }

        public void SetRasterizerState(bool wireframe = true)
        {
            var rs = new RasterizerState();

            //rs.CullMode = Microsoft.Xna.Framework.Graphics.CullMode.CullClockwiseFace;
            rs.CullMode = Microsoft.Xna.Framework.Graphics.CullMode.None;
            if (wireframe)
                rs.FillMode = FillMode.WireFrame;
            else
                rs.FillMode = FillMode.Solid;

            GraphicsDevice.RasterizerState = rs;
        }

        public void Draw(Dictionary<uint, R_Landblock> landblocks)
        {
            GraphicsDevice.Clear(new Color(32, 32, 32));
            SetRasterizerState(false);
            Effect.Parameters["xView"].SetValue(Camera.ViewMatrix);

            //landblock.Draw();
            Buffer.Draw();
        }

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

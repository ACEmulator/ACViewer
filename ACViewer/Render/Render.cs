using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            rs.CullMode = CullMode.None;

            if (wireframe)
                rs.FillMode = FillMode.WireFrame;
            else
                rs.FillMode = FillMode.Solid;

            GraphicsDevice.RasterizerState = rs;
        }

        private static readonly Color BackgroundColor = new Color(32, 32, 32);

        public void Draw(Dictionary<uint, R_Landblock> landblocks)
        {
            GraphicsDevice.Clear(BackgroundColor);
            SetRasterizerState(false);
            Effect.Parameters["xView"].SetValue(Camera.ViewMatrix);

            //landblock.Draw();
            Buffer.Draw();
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

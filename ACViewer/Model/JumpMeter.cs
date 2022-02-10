using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Model
{
    public class JumpMeter
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;
        
        public static SpriteBatch spriteBatch => GameView.Instance.SpriteBatch;

        public bool IsCharging { get; set; }

        public float Percent { get; set; }

        public static Texture2D TextureBlack { get; set; }

        public static Texture2D TextureYellow { get; set; }

        static JumpMeter()
        {
            var colors = new Color[1];
            colors[0] = Color.Black;
            
            TextureBlack = new Texture2D(GraphicsDevice, 1, 1);
            TextureBlack.SetData(colors);

            colors = new Color[1];
            colors[0] = Color.Yellow;

            TextureYellow = new Texture2D(GraphicsDevice, 1, 1);
            TextureYellow.SetData(colors);
        }

        public void Start()
        {
            Percent = 0.0f;
            IsCharging = true;
        }

        public void Stop()
        {
            IsCharging = false;
        }
        
        public void Update()
        {
            if (!IsCharging) return;

            Percent += 0.02f;

            if (Percent > 1.0f)
                Percent = 1.0f;
        }

        public void Draw()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp);
            var rect = new Rectangle(0, 0, 200, 10);
            spriteBatch.Draw(TextureBlack, rect, Color.White);
            rect = new Rectangle(0, 0, (int)(Percent * 200), 10);
            spriteBatch.Draw(TextureYellow, rect, Color.White);
            spriteBatch.End();
        }
    }
}

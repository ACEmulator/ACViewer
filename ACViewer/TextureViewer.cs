using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Framework.WpfInterop.Input;

using ACViewer.Render;

namespace ACViewer
{
    public class TextureViewer
    {
        public static TextureViewer Instance { get; set; }

        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;
        public SpriteBatch SpriteBatch => GameView.Instance.SpriteBatch;

        public static Effect Effect => Render.Render.Effect;

        public static uint FileID { get; set; }

        public static Texture2D Texture { get; set; }

        public WpfKeyboard Keyboard => GameView.Instance._keyboard;
        public WpfMouse Mouse => GameView.Instance._mouse;

        public KeyboardState PrevKeyboardState
        {
            get => GameView.Instance.PrevKeyboardState;
            set => GameView.Instance.PrevKeyboardState = value;
        }
        public MouseState PrevMouseState
        {
            get => GameView.Instance.PrevMouseState;
            set => GameView.Instance.PrevMouseState = value;
        }

        public Vector2 Pos { get; set; }
        public Matrix Translate { get; set; } = Matrix.Identity;

        public float CurScale { get; set; } = 1.0f;
        public Matrix Scale { get; set; } = Matrix.Identity;

        public Vector2 ImagePos { get; set; }

        public TextureViewer()
        {
            Instance = this;

            Effect.CurrentTechnique = Effect.Techniques["TexturedNoShading"];
        }

        public void LoadTexture(uint fileID)
        {
            // 0x08 - Surface - contains a 0x05 SurfaceTexture, along with additional type info (clipmask)
            // 0x05 - SurfaceTexture - contains a list of 0x06 textures
            // 0x06 - Texture - image format and data
            // 0x04 - Palette

            FileID = fileID;
            Texture = TextureCache.Get(fileID);
        }

        public static float Speed { get; set; } = 8.0f;

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            var offset = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.Left))
                offset.X += Speed;
            if (keyboardState.IsKeyDown(Keys.Right))
                offset.X -= Speed;
            if (keyboardState.IsKeyDown(Keys.Up))
                offset.Y += Speed;
            if (keyboardState.IsKeyDown(Keys.Down))
                offset.Y -= Speed;

            Pos += offset;

            if (mouseState.Position != PrevMouseState.Position)
                OnMouseMove(mouseState);

            if (mouseState.LeftButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed)
            {
                var delta = PrevMouseState.Position - mouseState.Position;
                Pos -= new Vector2(delta.X, delta.Y);
            }

            Translate = Matrix.CreateTranslation(Pos.X, Pos.Y, 0);

            // scaling
            if (mouseState.ScrollWheelValue != PrevMouseState.ScrollWheelValue)
            {
                var diff = mouseState.ScrollWheelValue - PrevMouseState.ScrollWheelValue;
                OnZoom(diff);
            }

            PrevKeyboardState = keyboardState;
            PrevMouseState = mouseState;
        }

        public void SetScale(float scale)
        {
            CurScale = scale;
            Scale = Matrix.CreateScale(CurScale);
        }

        public void OnMouseMove(MouseState mouseState)
        {
            var curPos = new Vector2(mouseState.Position.X, mouseState.Position.Y);

            if (curPos.X >= 0 && curPos.Y >= 0 && curPos.X < GraphicsDevice.Viewport.Width && curPos.Y < GraphicsDevice.Viewport.Height)
                GetCurrentPos(curPos);
        }

        public Vector2? GetCurrentPos(Vector2 curPos)
        {
            // get image pixel currently hovering over
            var imagePos = Vector2.Transform(curPos, Matrix.Invert(Scale * Translate));
            var curPixel = new Vector2((float)Math.Round(imagePos.X), (float)Math.Round(imagePos.Y));

            if (curPixel.X < 0 || curPixel.Y < 0 || curPixel.X >= Texture.Width || curPixel.Y >= Texture.Height)
                return null;

            return ImagePos = imagePos;
        }

        public static float ScaleStep { get; set; } = 0.85f;

        public void OnZoom(float scrollWheel)
        {
            if (scrollWheel < 0)
                CurScale *= ScaleStep;
            else
                CurScale /= ScaleStep;

            var beforePos = ImagePos;

            Scale = Matrix.CreateScale(CurScale);

            var mouseState = Mouse.GetState();
            var afterPos = GetCurrentPos(new Vector2(mouseState.Position.X, mouseState.Position.Y));

            if (afterPos == null)
                return;

            var delta = Vector2.Transform(afterPos.Value - beforePos, Scale);
            Pos += delta;
            Translate = Matrix.CreateTranslation(Pos.X, Pos.Y, 0);

            OnMouseMove(mouseState);
        }

        public void Draw(GameTime time)
        {
            if (Texture == null) return;

            GraphicsDevice.Clear(Color.Black);

            var samplerState = FileID >> 24 == 0x04 || FileID >> 24 == 0x0F ? SamplerState.PointClamp : SamplerState.LinearClamp;

            SpriteBatch.Begin(SpriteSortMode.Immediate, null, samplerState, null, null, null, Scale * Translate);

            SpriteBatch.Draw(Texture, Vector2.Zero, Color.White);

            SpriteBatch.End();
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACViewer.View;
using ACViewer.Model;
using ACViewer.Render;
using ACE.Entity.Enum;
using MonoGame.Framework.WpfInterop.Input;
using Microsoft.Xna.Framework.Input;

namespace ACViewer
{
    public class TextureViewer
    {
        public static MainWindow MainWindow { get => MainWindow.Instance; }

        public static TextureViewer Instance;

        public static GraphicsDevice GraphicsDevice { get => GameView.Instance.GraphicsDevice; }
        public Render.Render Render { get => GameView.Instance.Render; }
        public static Effect Effect { get => ACViewer.Render.Render.Effect; }
        public static Camera Camera { get => GameView.Camera; }

        public SpriteBatch SpriteBatch;

        public static uint FileID;
        public static Texture2D Texture;

        public WpfKeyboard Keyboard { get => GameView.Instance._keyboard; }
        public WpfMouse Mouse { get => GameView.Instance._mouse; }

        public KeyboardState PrevKeyboardState;
        public MouseState PrevMouseState;

        public Vector2 Pos;
        public Matrix Translate = Matrix.Identity;

        public float CurScale = 1.0f;
        public Matrix Scale = Matrix.Identity;

        public Vector2 ImagePos;

        public TextureViewer()
        {
            Instance = this;

            Effect.CurrentTechnique = Effect.Techniques["TexturedNoShading"];

            SpriteBatch = new SpriteBatch(GraphicsDevice);
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

        public static float Speed = 8.0f;
        public static float ScaleStep = 0.85f;

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.Left))
                Pos.X += Speed;
            if (keyboardState.IsKeyDown(Keys.Right))
                Pos.X -= Speed;
            if (keyboardState.IsKeyDown(Keys.Up))
                Pos.Y += Speed;
            if (keyboardState.IsKeyDown(Keys.Down))
                Pos.Y -= Speed;

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


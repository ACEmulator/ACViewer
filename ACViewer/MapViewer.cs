﻿using System;
using System.Drawing;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Framework.WpfInterop.Input;

using ACViewer.Enum;
using ACE.DatLoader;

namespace ACViewer
{
    public class MapViewer
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;
        public SpriteBatch spriteBatch => GameView.Instance.SpriteBatch;

        public static MapViewer Instance { get; set; }

        public Texture2D WorldMap { get; set; }

        public Vector2 Pos { get; set; }

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

        public float CurScale { get; set; } = 1.0f;

        public Matrix Scale { get; set; } = Matrix.Identity;

        public Matrix Translate { get; set; } = Matrix.Identity;

        public Vector2 ImagePos { get; set; }
        public Matrix BlockTranslate { get; set; }

        public Texture2D Highlight { get; set; }

        public bool IsDragging { get; set; }

        public bool DragCompleted { get; set; }

        public Vector2 StartPos { get; set; }
        public Vector2 EndPos { get; set; }

        public Microsoft.Xna.Framework.Rectangle HighlightRect { get; set; }

        public MapViewer()
        {
            Instance = this;

            LoadContent();
        }

        public void LoadContent()
        {
            if(WorldMap == null)
            {
                Mapper map = new Mapper();
                WorldMap = Image.GetTexture2DFromBitmap(GraphicsDevice, map.MapImage);
            }

            Highlight = new Texture2D(GraphicsDevice, 1, 1);
            Highlight.SetData(new Microsoft.Xna.Framework.Color[1] { Microsoft.Xna.Framework.Color.Red });
        }

        public void Init()
        {
            GameView.ViewMode = ViewMode.Map;

            DragCompleted = false;
        }

        public static float Speed { get; set; } = 8.0f;

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            var offset = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                offset.X += Speed;
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                offset.X -= Speed;
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
                offset.Y += Speed;
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
                offset.Y -= Speed;

            Pos += offset;
            
            Translate = Matrix.CreateTranslation(Pos.X, Pos.Y, 0);

            if (mouseState.Position != PrevMouseState.Position || offset != Vector2.Zero)
                OnMouseMove(mouseState);

            if (IsDragging && keyboardState.IsKeyDown(Keys.Escape))
                IsDragging = false;

            if (mouseState.LeftButton == ButtonState.Pressed && !DragCompleted)
            {
                if (!IsDragging)
                {
                    StartPos = ImagePos;
                    EndPos = ImagePos;
                    IsDragging = true;

                    var startBlock = GetLandblock(StartPos);
                    HighlightRect = new Microsoft.Xna.Framework.Rectangle((int)startBlock.X, (int)startBlock.Y, 8, 8);

                    BuildSelection();
                }
                else if (mouseState.Position != PrevMouseState.Position)
                {
                    EndPos = ImagePos;

                    GetMinMax(out var min, out var max);
                    var diff = max - min;

                    HighlightRect = new Microsoft.Xna.Framework.Rectangle((int)min.X, (int)min.Y, (int)diff.X + 8, (int)diff.Y + 8);

                    var upperLeftBlock = GetLandblock(min);
                    BlockTranslate = Matrix.CreateTranslation(upperLeftBlock.X, upperLeftBlock.Y, 0);

                    BuildSelection();
                }
            }
            else if (IsDragging)
            {
                OnMouseMove(mouseState);
                IsDragging = false;
                DragCompleted = true;

                GetMinMax(out var min, out var max);
                var startBlock = GetLandblock(min) / 8;
                var endBlock = GetLandblock(max) / 8;

                // reverse y
                startBlock.Y = 254 - startBlock.Y;
                endBlock.Y = 254 - endBlock.Y;

                var startBlockID = (uint)startBlock.X << 24 | (uint)startBlock.Y << 16 | 0xFFFF;
                var endBlockID = (uint)endBlock.X << 24 | (uint)endBlock.Y << 16 | 0xFFFF;

                // cap size / warning?

                //Console.WriteLine($"StartBlock: {startBlockID:X8}");
                //Console.WriteLine($"EndBlock: {endBlockID:X8}");

                WorldViewer.Instance = new WorldViewer();
                WorldViewer.Instance.LoadLandblocks(startBlock, endBlock);
            }

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                var delta = PrevMouseState.Position - mouseState.Position;
                Pos -= new Vector2(delta.X, delta.Y);
            }

            // scaling
            if (mouseState.ScrollWheelValue != PrevMouseState.ScrollWheelValue)
            {
                var diff = mouseState.ScrollWheelValue - PrevMouseState.ScrollWheelValue;
                OnZoom(diff);
            }

            PrevKeyboardState = keyboardState;
            PrevMouseState = mouseState;
        }

        public void GetMinMax(out Vector2 min, out Vector2 max)
        {
            // get mins/maxs
            min = new Vector2(float.MaxValue, float.MaxValue);
            max = new Vector2(float.MinValue, float.MinValue);

            var startBlock = GetLandblock(StartPos);
            var endBlock = GetLandblock(EndPos);

            GetMinMax_Inner(ref min, ref max, startBlock);
            GetMinMax_Inner(ref min, ref max, endBlock);
        }

        public void GetMinMax_Inner(ref Vector2 mins, ref Vector2 maxs, Vector2 p)
        {
            if (p.X < mins.X) mins.X = p.X;
            if (p.Y < mins.Y) mins.Y = p.Y;
            if (p.X > maxs.X) maxs.X = p.X;
            if (p.Y > maxs.Y) maxs.Y = p.Y;
        }

        public void OnMouseMove(MouseState mouseState)
        {
            if (mouseState.LeftButton != ButtonState.Pressed && IsDragging || DragCompleted)
                return;
            
            var curPos = new Vector2(mouseState.Position.X, mouseState.Position.Y);

            if (curPos.X >= 0 && curPos.Y >= 0 && curPos.X < GraphicsDevice.Viewport.Width && curPos.Y < GraphicsDevice.Viewport.Height)
                GetCurrentPos(curPos);
        }

        public Vector2 GetCurrentPos(Vector2 curPos)
        {
            // get image pixel currently hovering over
            var imagePos = Vector2.Transform(curPos, Matrix.Invert(Scale * Translate));
            var curPixel = new Vector2((float)Math.Round(imagePos.X), (float)Math.Round(imagePos.Y));

            if (curPixel.X < 0 || curPixel.Y < 0 || curPixel.X > WorldMap.Width || curPixel.Y > WorldMap.Height)
                return imagePos;

            ImagePos = imagePos;

            var curLandblock = GetLandblock(ImagePos);
            BlockTranslate = Matrix.CreateTranslation(curLandblock.X, curLandblock.Y, 0);

            //Console.WriteLine($"Window pos: {curPos}, Image pos: {ImagePos}");
            return imagePos;
        }

        public Vector2 GetLandblock(Vector2 imagePos)
        {
            return new Vector2((int)Math.Min(254, Math.Round(imagePos.X) / 8) * 8, (int)Math.Min(254, Math.Round(imagePos.Y) / 8) * 8);
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

            var delta = Vector2.Transform(afterPos - beforePos, Scale);
            Pos += delta;
            Translate = Matrix.CreateTranslation(Pos.X, Pos.Y, 0);

            OnMouseMove(mouseState);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Scale * Translate);
            spriteBatch.Draw(WorldMap, Vector2.Zero, Microsoft.Xna.Framework.Color.White);
            spriteBatch.End();

            DrawHighlight();

            DrawHUD();
        }

        public static Microsoft.Xna.Framework.Rectangle[] Highlight_Sides { get; } = new Microsoft.Xna.Framework.Rectangle[4]
        {
            new Microsoft.Xna.Framework.Rectangle(0, 0, 8, 1),
            new Microsoft.Xna.Framework.Rectangle(0, 7, 8, 1),
            new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 8),
            new Microsoft.Xna.Framework.Rectangle(7, 0, 1, 8),
        };

        public Microsoft.Xna.Framework.Rectangle[] Selection { get; set; }

        public void BuildSelection()
        {
            var width = HighlightRect.Width;
            var height = HighlightRect.Height;

            Selection = new Microsoft.Xna.Framework.Rectangle[4]
            {
                new Microsoft.Xna.Framework.Rectangle(0, 0, width, 1),
                new Microsoft.Xna.Framework.Rectangle(0, height - 1, width, 1),
                new Microsoft.Xna.Framework.Rectangle(0, 0, 1, height),
                new Microsoft.Xna.Framework.Rectangle(width - 1, 0, 1, height),
            };
        }

        public void DrawHighlight()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointWrap, null, null, null, BlockTranslate * Scale * Translate);

            if (!IsDragging && !DragCompleted)
            {
                foreach (var side in Highlight_Sides)
                    spriteBatch.Draw(Highlight, side, Microsoft.Xna.Framework.Color.White);
            }
            else
            {
                foreach (var side in Selection)
                    spriteBatch.Draw(Highlight, side, Microsoft.Xna.Framework.Color.White);
            }

            spriteBatch.End();
        }

        // text rendering
        public SpriteFont Font => GameView.Instance.Font;

        public void DrawHUD()
        {
            var landblock = ImagePos / 8;

            if (landblock.X >= 0 && landblock.X < 255 && landblock.Y >= 0 && landblock.Y < 255)
            {
                landblock.Y = 255 - landblock.Y;

                var textPos = new Vector2(GraphicsDevice.Viewport.Width - 42, 10);

                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp);
                spriteBatch.DrawString(Font, $"{(int)landblock.X:X2}{(int)landblock.Y:X2}", textPos, Microsoft.Xna.Framework.Color.White);
                spriteBatch.End();
            }
        }
    }
}

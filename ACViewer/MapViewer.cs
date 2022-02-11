using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Framework.WpfInterop.Input;

using ACViewer.Config;
using ACViewer.Enum;
using ACViewer.View;

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

        public KeyboardState PrevKeyboardState => GameView.Instance.PrevKeyboardState;

        public MouseState PrevMouseState => GameView.Instance.PrevMouseState;

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

        public Rectangle HighlightRect { get; set; }

        public Texture2D GearIcon { get; set; }

        public MapViewer()
        {
            Instance = this;

            LoadContent();
        }

        public void LoadContent()
        {
            LoadMap();

            Highlight = new Texture2D(GraphicsDevice, 1, 1);
            Highlight.SetData(new Color[1] { Color.Red });

            var stream = Application.GetResourceStream(new Uri("Icons/Settings_16x_inverted.png", UriKind.Relative));
            GearIcon = Texture2D.FromStream(GraphicsDevice, stream.Stream);
        }

        public void LoadMap()
        {
            if (WorldMap != null)
            {
                WorldMap.Dispose();
                WorldMap = null;
            }

            var worker = new BackgroundWorker();

            worker.DoWork += (sender, doWorkEventArgs) =>
            {
                switch (ConfigManager.Config.MapViewer.Mode)
                {
                    case MapViewerMode.PreGenerated:
                        WorldMap = Image.GetTextureFromBitmap(GraphicsDevice, @"Content\Images\highres.png");
                        break;

                    case MapViewerMode.GenerateFromDAT:
                        var mapper = new Mapper();
                        WorldMap = Image.GetTexture2DFromBitmap(GraphicsDevice, mapper.MapImage.Bitmap);
                        break;
                }
            };
            worker.RunWorkerAsync();
        }

        public void Init()
        {
            GameView.ViewMode = ViewMode.Map;

            DragCompleted = false;
        }

        public static float Speed { get; set; } = 8.0f;

        public bool OptionsActive { get; set; }

        public DateTime OptionsLastClosedTime { get; set; }

        public void Update(GameTime gameTime)
        {
            if (WorldMap == null) return;
            
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
                if (!ShouldDrawHighlight())
                {
                    if (!OptionsActive && DateTime.Now - OptionsLastClosedTime > TimeSpan.FromSeconds(0.25))
                    {
                        System.Windows.Input.Mouse.OverrideCursor = null;
                        OptionsActive = true;
                        var options = new Options_MapViewer();
                        options.WindowStartupLocation = WindowStartupLocation.Manual;

                        // get absolute screen coordnate of upper left pixel of control
                        var startPos = MainWindow.Instance.Scene.PointToScreen(new System.Windows.Point(0, 0));

                        options.Top = startPos.Y + MainWindow.Instance.Scene.ActualHeight / 2 - options.Height / 2;
                        options.Left = startPos.X + MainWindow.Instance.Scene.ActualWidth / 2 - options.Width / 2;

                        options.ShowDialog();

                        OptionsLastClosedTime = DateTime.Now;
                        OptionsActive = false;
                    }
                    return;
                }

                if (!IsDragging)
                {
                    StartPos = ImagePos;
                    EndPos = ImagePos;
                    IsDragging = true;

                    var startBlock = GetLandblock(StartPos);
                    HighlightRect = new Rectangle((int)startBlock.X, (int)startBlock.Y, 8, 8);

                    BuildSelection();
                }
                else if (mouseState.Position != PrevMouseState.Position)
                {
                    EndPos = ImagePos;

                    GetMinMax(out var min, out var max);
                    var diff = max - min;

                    HighlightRect = new Rectangle((int)min.X, (int)min.Y, (int)diff.X + 8, (int)diff.Y + 8);

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

                //WorldViewer.Instance = new WorldViewer();
                WorldViewer.Instance.LoadLandblocks(startBlock, endBlock);
            }

            if (mouseState.RightButton == ButtonState.Pressed && PrevMouseState.RightButton == ButtonState.Pressed)
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

            ManageCursor();
        }

        public void ManageCursor()
        {
            if (ShouldDrawHighlight())
                System.Windows.Input.Mouse.OverrideCursor = null;
            else
                System.Windows.Input.Mouse.OverrideCursor = Cursors.Hand;
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
            GraphicsDevice.Clear(ConfigManager.Config.BackgroundColors.TextureViewer);

            if (WorldMap == null) return;

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Scale * Translate);
            spriteBatch.Draw(WorldMap, Vector2.Zero, Color.White);
            spriteBatch.End();

            DrawHighlight();

            DrawHUD();
        }

        public static Rectangle[] Highlight_Sides { get; } = new Rectangle[4]
        {
            new Rectangle(0, 0, 8, 1),
            new Rectangle(0, 7, 8, 1),
            new Rectangle(0, 0, 1, 8),
            new Rectangle(7, 0, 1, 8),
        };

        public Rectangle[] Selection { get; set; }

        public void BuildSelection()
        {
            var width = HighlightRect.Width;
            var height = HighlightRect.Height;

            Selection = new Rectangle[4]
            {
                new Rectangle(0, 0, width, 1),
                new Rectangle(0, height - 1, width, 1),
                new Rectangle(0, 0, 1, height),
                new Rectangle(width - 1, 0, 1, height),
            };
        }

        public void DrawHighlight()
        {
            if (!ShouldDrawHighlight())
                return;
            
            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointWrap, null, null, null, BlockTranslate * Scale * Translate);

            if (!IsDragging && !DragCompleted)
            {
                foreach (var side in Highlight_Sides)
                    spriteBatch.Draw(Highlight, side, Color.White);
            }
            else
            {
                foreach (var side in Selection)
                    spriteBatch.Draw(Highlight, side, Color.White);
            }

            spriteBatch.End();
        }

        // text rendering
        public SpriteFont Font => GameView.Instance.Font;

        private static readonly int gearIconPadding = 10;

        private static readonly int gearIconSize = 16;

        public void DrawHUD()
        {
            var landblock = ImagePos / 8;

            if (landblock.X >= 0 && landblock.X < 255 && landblock.Y >= 0 && landblock.Y < 255)
            {
                landblock.Y = 255 - landblock.Y;

                var textPos = new Vector2(GraphicsDevice.Viewport.Width - 42, 10);

                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp);
                spriteBatch.DrawString(Font, $"{(int)landblock.X:X2}{(int)landblock.Y:X2}", textPos, Color.White);
                spriteBatch.End();
            }

            // draw gear icon in bottom-right corner
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp);
            var rect = new Rectangle(GraphicsDevice.Viewport.Width - GearIcon.Width - gearIconPadding, GraphicsDevice.Viewport.Height - GearIcon.Height - gearIconPadding, gearIconSize, gearIconSize);
            spriteBatch.Draw(GearIcon, rect, Color.White);
            spriteBatch.End();
        }

        public bool ShouldDrawHighlight()
        {
            var mouseState = Mouse.GetState();

            if (mouseState.X > GraphicsDevice.Viewport.Width - gearIconSize - gearIconPadding * 2 && mouseState.Y > GraphicsDevice.Viewport.Height - gearIconSize - gearIconPadding * 2)
                return false;

            return true;
        }
    }
}

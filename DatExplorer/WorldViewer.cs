using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop.Input;
using ACE.Server.Physics;
using ACE.Server.Physics.Common;
using DatExplorer.Render;
using DatExplorer.View;

namespace DatExplorer
{
    public class WorldViewer
    {
        public static MainWindow MainWindow { get => MainWindow.Instance; }
        public static Render.Render Render { get => GameView.Instance.Render; }

        public static WorldViewer Instance;

        public Dictionary<uint, R_Landblock> Landblocks;

        public PhysicsEngine Physics;

        public Render.Buffer Buffer { get => Render.Buffer; }

        public static Camera Camera { get => GameView.Camera; }

        public WpfKeyboard Keyboard { get => GameView.Instance._keyboard; }
        public WpfMouse Mouse { get => GameView.Instance._mouse; }

        public KeyboardState PrevKeyboardState;

        public bool DungeonMode = false;

        public WorldViewer()
        {
            Instance = this;
            Physics = new PhysicsEngine(new ObjectMaint(), new SmartBox());
            Physics.Server = false;
        }

        public void LoadLandblock(uint landblockID, uint radius = 1)
        {
            Buffer.Init();
            TextureCache.Init();

            var landblock = LScape.get_landblock(landblockID);
            if (landblock.IsDungeon)
                radius = 0;

            DungeonMode = landblock.IsDungeon;

            var center_lbx = landblockID >> 24;
            var center_lby = landblockID >> 16 & 0xFF;

            Landblocks = new Dictionary<uint, R_Landblock>();
            for (var lbx = (int)(center_lbx - radius); lbx <= center_lbx + radius; lbx++)
            {
                if (lbx < 0 || lbx > 254) continue;

                for (var lby = (int)(center_lby - radius); lby <= center_lby + radius; lby++)
                {
                    if (lby < 0 || lby > 254) continue;

                    var lbid = (uint)(lbx << 24 | lby << 16 | 0xFFFF);

                    var timer = Stopwatch.StartNew();
                    landblock = LScape.get_landblock(lbid);
                    timer.Stop();

                    MainWindow.Status.WriteLine($"Loaded {lbid:X8} in {timer.Elapsed.TotalMilliseconds}ms");

                    Landblocks.Add(lbid, new R_Landblock(landblock));
                }
            }

            Buffer.BuildBuffers();

            Camera.InitLandblock(Landblocks[landblockID]);
        }

        public async void LoadLandblocks(Vector2 startBlock, Vector2 endBlock)
        {
            Buffer.Init();
            TextureCache.Init();

            DungeonMode = false;

            var dx = (int)(endBlock.X - startBlock.X) + 1;
            var dy = (int)(startBlock.Y - endBlock.Y) + 1;
            var numBlocks = dx * dy;

            MainWindow.Status.WriteLine($"Loading {numBlocks} landblocks");
            await Task.Run(() => Thread.Sleep(50));

            Landblocks = new Dictionary<uint, R_Landblock>();
            for (var lbx = (uint)startBlock.X; lbx <= endBlock.X; lbx++)
            {
                if (lbx < 0 || lbx > 254) continue;

                for (var lby = (uint)endBlock.Y; lby <= startBlock.Y; lby++)
                {
                    if (lby < 0 || lby > 254) continue;

                    var lbid = lbx << 24 | lby << 16 | 0xFFFF;

                    var timer = Stopwatch.StartNew();
                    var landblock = LScape.get_landblock(lbid);
                    timer.Stop();

                    MainWindow.Status.WriteLine($"Loaded {lbid:X8} in {timer.Elapsed.TotalMilliseconds}ms");

                    Landblocks.Add(lbid, new R_Landblock(landblock));
                }
            }

            Buffer.BuildBuffers();

            var size = endBlock - startBlock;
            var center = startBlock + size / 2;
            var centerX = (uint)Math.Round(center.X);
            var centerY = (uint)Math.Round(center.Y);
            var centerBlock = centerX << 24 | centerY << 16 | 0xFFFF;

            Camera.InitLandblock(Landblocks[centerBlock]);
            GameView.ViewMode = ViewMode.World;
        }

        public void ShowLoadStatus(int numBlocks)
        {
            
        }

        public void Update(GameTime time)
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.M) && !PrevKeyboardState.IsKeyDown(Keys.M))
            {
                Console.WriteLine("Map");
                GameView.ViewMode = ViewMode.Map;
            }

            PrevKeyboardState = keyboardState;

            if (Camera != null)
                Camera.Update(time);

            DrawCount.Update();
        }

        public void Draw(GameTime time)
        {
            Render.Draw(Landblocks);
        }
    }
}

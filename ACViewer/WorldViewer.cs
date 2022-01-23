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

using ACViewer.Enum;
using ACViewer.Render;
using ACViewer.View;

namespace ACViewer
{
    public class WorldViewer
    {
        public static MainWindow MainWindow => MainWindow.Instance;

        public static WorldViewer Instance { get; set; }

        public static Render.Render Render => GameView.Instance.Render;

        public Render.Buffer Buffer => Render.Buffer;
        
        public static Camera Camera => GameView.Camera;

        public PhysicsEngine Physics { get; set; }

        public WpfKeyboard Keyboard => GameView.Instance._keyboard;

        public KeyboardState PrevKeyboardState
        {
            get => GameView.Instance.PrevKeyboardState;
            set => GameView.Instance.PrevKeyboardState = value;
        }

        public bool DungeonMode { get; set; }

        public Model.BoundingBox BoundingBox { get; set; }

        public WorldViewer()
        {
            Instance = this;
            
            Physics = new PhysicsEngine(new ObjectMaint(), new SmartBox());
            
            Physics.Server = false;
        }

        public void LoadLandblock(uint landblockID, uint radius = 1)
        {
            Render.Buffer.ClearBuffer();
            TextureCache.Init();

            LScape.unload_landblocks_all();

            var landblock = LScape.get_landblock(landblockID);
            if (landblock.HasDungeon)
                radius = 0;

            DungeonMode = landblock.HasDungeon;

            var center_lbx = landblockID >> 24;
            var center_lby = landblockID >> 16 & 0xFF;

            //Landblocks = new Dictionary<uint, R_Landblock>();
            R_Landblock r_landblock = null;
            R_Landblock centerBlock = null;

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

                    r_landblock = new R_Landblock(landblock);
                    //LScape.unload_landblock(lbid);

                    if (landblockID == lbid)
                        centerBlock = r_landblock;

                    MainWindow.Status.WriteLine($"Loaded {lbid:X8} in {timer.Elapsed.TotalMilliseconds}ms");

                    //Landblocks.Add(lbid, new R_Landblock(landblock));
                }
            }

            Render.Buffer.BuildBuffers();
            Render.InitEmitters();

            if (FileExplorer.Instance.TeleportMode)
            {
                var zBump = DungeonMode ? 1.775f : 2.775f;
                
                Camera.InitTeleport(centerBlock, zBump);
                FileExplorer.Instance.TeleportMode = false;
            }
            else if (DungeonMode)
            {
                BoundingBox = new Model.BoundingBox(Render.Buffer.RB_EnvCell);
                Camera.InitDungeon(r_landblock, BoundingBox);
            }
            else
                Camera.InitLandblock(r_landblock);

            FreeResources();
        }

        public async void LoadLandblocks(Vector2 startBlock, Vector2 endBlock)
        {
            Render.Buffer.ClearBuffer();
            TextureCache.Init();
            
            LScape.unload_landblocks_all();

            DungeonMode = false;

            var dx = (int)(endBlock.X - startBlock.X) + 1;
            var dy = (int)(startBlock.Y - endBlock.Y) + 1;
            var numBlocks = dx * dy;

            var size = endBlock - startBlock;
            var center = startBlock + size / 2;
            var centerX = (uint)Math.Round(center.X);
            var centerY = (uint)Math.Round(center.Y);
            var landblockID = centerX << 24 | centerY << 16 | 0xFFFF;

            MainWindow.Status.WriteLine($"Loading {numBlocks} landblocks");
            await Task.Run(() => Thread.Sleep(50));

            //Landblocks = new Dictionary<uint, R_Landblock>();
            R_Landblock r_landblock = null;
            R_Landblock centerBlock = null;

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

                    r_landblock = new R_Landblock(landblock);
                    //LScape.unload_landblock(lbid);

                    if (lbid == landblockID)
                        centerBlock = r_landblock;

                    MainWindow.Status.WriteLine($"Loaded {lbid:X8} in {timer.Elapsed.TotalMilliseconds}ms");

                    //Landblocks.Add(lbid, new R_Landblock(landblock));
                }
            }

            Render.Buffer.BuildBuffers();
            Render.InitEmitters();

            Camera.InitLandblock(centerBlock);
            GameView.ViewMode = ViewMode.World;

            FreeResources();
        }

        public void FreeResources()
        {
            R_Landblock.Init();
            TextureCache.Init(false);
        }

        public void Update(GameTime time)
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.M) && !PrevKeyboardState.IsKeyDown(Keys.M))
            {
                MapViewer.Instance.Init();
            }

            if (keyboardState.IsKeyDown(Keys.H) && !PrevKeyboardState.IsKeyDown(Keys.H))
            {
                MainMenu.ToggleHUD();
            }

            if (keyboardState.IsKeyDown(Keys.L) && !PrevKeyboardState.IsKeyDown(Keys.L))
            {
                ShowLocation();
            }

            PrevKeyboardState = keyboardState;

            if (Camera != null)
                Camera.Update(time);

            Render.UpdateEmitters();

            if (PerfTimer.Update())
            {
                //Console.WriteLine($"NumParticles: {ACViewer.Render.Render.NumParticlesThisFrame}, ParticleTextures: {ACViewer.Render.Render.ParticleTexturesThisFrame.Count}");
            }
        }

        public void ShowLocation()
        {
            var pos = Camera.GetPosition() ?? "unknown";
            MainWindow.Status.WriteLine($"Location: {pos}");
        }

        public void Draw(GameTime time)
        {
            //Render.Draw(Landblocks);
            Render.Draw();

            if (MainMenu.ShowHUD)
                Render.DrawHUD();
        }
    }
}

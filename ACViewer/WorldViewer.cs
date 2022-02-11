﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public KeyboardState PrevKeyboardState => GameView.Instance.PrevKeyboardState;

        public bool DungeonMode { get; set; }

        public Model.BoundingBox BoundingBox { get; set; }

        public uint SingleBlock { get; set; }

        public bool InitPlayerMode { get; set; }
        
        public WorldViewer()
        {
            if (Instance != null && Instance.PlayerMode)
            {
                Instance.ExitPlayerMode();
                InitPlayerMode = true;
            }

            Instance = this;
        }

        public void LoadLandblock(uint landblockID, uint radius = 1)
        {
            if (PlayerMode)
            {
                ExitPlayerMode();
                InitPlayerMode = true;
            }

            Render.Buffer.ClearBuffer();
            Server.Init();
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

                if (InitPlayerMode)
                    zBump = 0.0f;
                
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

            SingleBlock = landblock.ID;

            FreeResources();

            Server.OnLoadWorld();
        }

        public async void LoadLandblocks(Vector2 startBlock, Vector2 endBlock)
        {
            //Console.WriteLine($"LoadLandblocks({startBlock}, {endBlock})");
            if (PlayerMode)
            {
                ExitPlayerMode();
                InitPlayerMode = true;
            }

            Render.Buffer.ClearBuffer();
            Server.Init();
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
            await Task.Delay(1);

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

            SingleBlock = uint.MaxValue;

            FreeResources();

            Server.OnLoadWorld();
        }

        public void FreeResources()
        {
            TextureCache.Init(false);
        }

        public bool PlayerMode { get; set; }

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

            if (keyboardState.IsKeyDown(Keys.C) && !PrevKeyboardState.IsKeyDown(Keys.C))
            {
                Picker.ClearSelection();
            }

            if (GameView.ViewMode == ViewMode.World && (keyboardState.IsKeyDown(Keys.P) && !PrevKeyboardState.IsKeyDown(Keys.P) || InitPlayerMode))
            {
                if (!PlayerMode)
                    EnterPlayerMode();
                else
                    ExitPlayerMode();

                InitPlayerMode = false;
            }

            if (keyboardState.IsKeyDown(Keys.D1) && !PrevKeyboardState.IsKeyDown(Keys.D1))
            {
                ACViewer.Render.Buffer.drawTerrain = !ACViewer.Render.Buffer.drawTerrain;
            }

            if (keyboardState.IsKeyDown(Keys.D2) && !PrevKeyboardState.IsKeyDown(Keys.D2))
            {
                ACViewer.Render.Buffer.drawEnvCells = !ACViewer.Render.Buffer.drawEnvCells;
            }

            if (keyboardState.IsKeyDown(Keys.D3) && !PrevKeyboardState.IsKeyDown(Keys.D3))
            {
                ACViewer.Render.Buffer.drawStaticObjs = !ACViewer.Render.Buffer.drawStaticObjs;
            }

            if (keyboardState.IsKeyDown(Keys.D4) && !PrevKeyboardState.IsKeyDown(Keys.D4))
            {
                ACViewer.Render.Buffer.drawBuildings = !ACViewer.Render.Buffer.drawBuildings;
            }

            if (keyboardState.IsKeyDown(Keys.D5) && !PrevKeyboardState.IsKeyDown(Keys.D5))
            {
                ACViewer.Render.Buffer.drawScenery = !ACViewer.Render.Buffer.drawScenery;
            }

            if (keyboardState.IsKeyDown(Keys.D6) && !PrevKeyboardState.IsKeyDown(Keys.D6))
            {
                MainMenu.ToggleParticles();
            }

            if (keyboardState.IsKeyDown(Keys.D7) && !PrevKeyboardState.IsKeyDown(Keys.D7))
            {
                ACViewer.Render.Buffer.drawInstances = !ACViewer.Render.Buffer.drawInstances;
            }

            if (keyboardState.IsKeyDown(Keys.D8) && !PrevKeyboardState.IsKeyDown(Keys.D8))
            {
                ACViewer.Render.Buffer.drawEncounters = !ACViewer.Render.Buffer.drawEncounters;
            }

            if (keyboardState.IsKeyDown(Keys.D0) && !PrevKeyboardState.IsKeyDown(Keys.D0))
            {
                ACViewer.Render.Buffer.drawAlpha = !ACViewer.Render.Buffer.drawAlpha;
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.V) && !PrevKeyboardState.IsKeyDown(Keys.V))
            {
                Picker.AddVisibleCells();
            }

            if (GameView.ViewMode == ViewMode.World && PlayerMode && Player != null)
                Player.Update(time);
            else if (Camera != null)
                Camera.Update(time);

            Render.UpdateEmitters();

            Server.Update();

            if (PerfTimer.Update())
            {
                //Console.WriteLine($"NumParticles: {ACViewer.Render.Render.NumParticlesThisFrame}, ParticleTextures: {ACViewer.Render.Render.ParticleTexturesThisFrame.Count}");
            }
        }

        public void ShowLocation()
        {
            var pos = Camera.GetPosition();

            MainWindow.Status.WriteLine($"Location: {pos?.ToString() ?? "unknown"}");
        }

        public void Draw(GameTime time)
        {
            //Render.Draw(Landblocks);
            Render.Draw();

            if (MainMenu.ShowHUD)
                Render.DrawHUD();

            if (Player != null && PlayerMode)
                Player.Draw();
        }

        public Player Player { get; set; }

        public bool EnterPlayerMode()
        {
            Player = new Player(true);

            // location = current camera location
            var cameraPos = Camera.GetPosition();

            if (cameraPos == null)
            {
                Console.WriteLine($"WorldViewer.EnterPlayerMode() - camera position null!");
                return false;
            }

            var success = Player.WorldObject.AddPhysicsObj(cameraPos);

            if (!success)
            {
                Console.WriteLine($"WorldViewer.EnterPlayerMode() - AddPhysicsObj({cameraPos}) failed");
                return false;
            }

            var r_PhysicsObj = new R_PhysicsObj(Player.PhysicsObj);
            Buffer.AddPlayer(r_PhysicsObj);

            Buffer.BuildTextureAtlases(Buffer.AnimatedTextureAtlasChains);
            Buffer.BuildBuffer(Buffer.RB_Animated);

            Camera.Locked = true;

            PlayerMode = true;
            
            return true;
        }

        public void ExitPlayerMode()
        {
            Camera.Locked = false;
            Camera.Dir = Vector3.Transform(Vector3.UnitY, Player.PhysicsObj.Position.Frame.Orientation.ToXna());
            PlayerMode = false;
            Player = null;

            Buffer.ClearBuffer(Buffer.RB_Animated);
            Buffer.RB_Animated = new Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared>();

            // clean this up
            if (GameView.WorldViewer != null && GameView.WorldViewer != this)
                GameView.WorldViewer.ExitPlayerMode();
        }
    }
}

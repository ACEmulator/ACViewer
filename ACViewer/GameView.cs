using System;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using ACViewer.View;

namespace ACViewer
{
    public enum ViewMode
    {
        None,
        Texture,
        Model,
        World,
        Map,
        Particle,
    };

    public class GameView : WpfGame
    {
        public WpfGraphicsDeviceService _graphicsDeviceManager;
        public SpriteBatch SpriteBatch;

        public WpfKeyboard _keyboard;
        public WpfMouse _mouse;

        public KeyboardState PrevKeyboardState;

        public static GameView Instance;

        public new Render.Render Render;
        public static Camera Camera;

        public Player Player;

        public static MainWindow Window { get => MainWindow.Instance; }

        public static WorldViewer WorldViewer;
        public static MapViewer MapViewer;
        public static ModelViewer ModelViewer;
        public static TextureViewer TextureViewer;
        public static ParticleViewer ParticleViewer;

        private static ViewMode _viewMode;

        public static ViewMode ViewMode
        {
            get => _viewMode;
            set
            {
                if (_viewMode != value)
                {
                    _viewMode = value;
                    if (_viewMode == ViewMode.Model)
                    {
                        Camera.Position = new Vector3(-10, -10, 10);
                        Camera.Dir = Vector3.Normalize(-Camera.Position);
                        Camera.Speed = Camera.Model_Speed;
                        Camera.SetNearPlane(Camera.NearPlane_Model);
                    }
                    if (_viewMode == ViewMode.Particle)
                    {
                        Camera.InitParticle();
                    }
                    if (_viewMode == ViewMode.World)
                    {
                        Camera.SetNearPlane(Camera.NearPlane_World);
                    }
                }
            }
        }

        public static bool UseMSAA = true;

        public DateTime LastResizeEvent;

        protected override void Initialize()
        {
            // must be initialized. required by Content loading and rendering (will add itself to the Services)
            // note that MonoGame requires this to be initialized in the constructor, while WpfInterop requires it to
            // be called inside Initialize (before base.Initialize())
            //var dummy = new DummyView();
            _graphicsDeviceManager = new WpfGraphicsDeviceService(this)
            {
                PreferMultiSampling = UseMSAA
            };

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            
            // wpf and keyboard need reference to the host control in order to receive input
            // this means every WpfGame control will have it's own keyboard & mouse manager which will only react if the mouse is in the control
            _keyboard = new WpfKeyboard(this);
            _mouse = new WpfMouse(this);

            Instance = this;

            // must be called after the WpfGraphicsDeviceService instance was created
            base.Initialize();

            SizeChanged += new SizeChangedEventHandler(GameView_SizeChanged);
        }

        public void PostInit()
        {
            InitPlayer();

            Render = new Render.Render();

            WorldViewer = new WorldViewer();
            MapViewer = new MapViewer();
            ModelViewer = new ModelViewer();
            TextureViewer = new TextureViewer();
            ParticleViewer = new ParticleViewer();
        }

        public void InitPlayer()
        {
            Player = new Player();
        }

        protected override void Update(GameTime time)
        {
            // every update we can now query the keyboard & mouse for our WpfGame
            var mouseState = _mouse.GetState();
            var keyboardState = _keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.C) && !PrevKeyboardState.IsKeyDown(Keys.C))
            {
                // cancel all emitters in progress
                Player.PhysicsObj.ParticleManager.ParticleTable.Clear();
            }

            if (!_graphicsDeviceManager.PreferMultiSampling && UseMSAA && DateTime.Now - LastResizeEvent >= TimeSpan.FromSeconds(1))
            {
                _graphicsDeviceManager.PreferMultiSampling = true;
                _graphicsDeviceManager.ApplyChanges();
            }

            PrevKeyboardState = keyboardState;

            if (Player != null)
                Player.Update(time);

            switch (ViewMode)
            {
                case ViewMode.Texture:
                    TextureViewer.Update(time);
                    break;
                case ViewMode.Model:
                    ModelViewer.Update(time);
                    break;
                case ViewMode.World:
                    WorldViewer.Update(time);
                    break;
                case ViewMode.Map:
                    MapViewer.Update(time);
                    break;
                case ViewMode.Particle:
                    ParticleViewer.Update(time);
                    break;
            }
        }

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(new Color(0, 0, 0));

            switch (ViewMode)
            {
                case ViewMode.Texture:
                    TextureViewer.Draw(time);
                    break;
                case ViewMode.Model:
                    ModelViewer.Draw(time);
                    break;
                case ViewMode.World:
                    WorldViewer.Draw(time);
                    break;
                case ViewMode.Map:
                    MapViewer.Draw(time);
                    break;
                case ViewMode.Particle:
                    ParticleViewer.Draw(time);
                    break;
            }
        }

        private void GameView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_graphicsDeviceManager.PreferMultiSampling)
            {
                _graphicsDeviceManager.PreferMultiSampling = false;
                _graphicsDeviceManager.ApplyChanges();
                LastResizeEvent = DateTime.Now;
            }
        }
    }
}

using System;
using System.Windows;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;

using ACViewer.Enum;

namespace ACViewer
{
    public class GameView : WpfGame
    {
        public static GameView Instance { get; set; }

        public WpfGraphicsDeviceService _graphicsDeviceManager { get; set; }
        public SpriteBatch SpriteBatch { get; set; }

        public WpfKeyboard _keyboard { get; set; }
        public WpfMouse _mouse { get; set; }

        public KeyboardState PrevKeyboardState { get; set; }
        public MouseState PrevMouseState { get; set; }

        public new Render.Render Render { get; set; }
        
        public static Camera Camera { get; set; }

        public Player Player { get; set; }

        public static WorldViewer WorldViewer { get; set; }
        public static MapViewer MapViewer { get; set; }
        public static ModelViewer ModelViewer { get; set; }
        public static TextureViewer TextureViewer { get; set; }
        public static ParticleViewer ParticleViewer { get; set; }

        private static ViewMode _viewMode { get; set; }

        public static ViewMode ViewMode
        {
            get => _viewMode;
            set
            {
                if (_viewMode == value) return;
                
                _viewMode = value;

                if (_viewMode == ViewMode.Model)
                {
                    Camera.Position = new Vector3(-10, -10, 10);
                    Camera.Dir = Vector3.Normalize(-Camera.Position);
                    Camera.Speed = Camera.Model_Speed;
                    Camera.SetNearPlane(Camera.NearPlane_Model);
                }
                else if (_viewMode == ViewMode.Particle)
                {
                    Camera.InitParticle();
                }
                else if (_viewMode == ViewMode.World)
                {
                    Camera.SetNearPlane(Camera.NearPlane_World);
                }
            }
        }

        public static bool UseMSAA { get; set; } = true;

        public DateTime LastResizeEvent { get; set; }

        // text rendering
        public SpriteFont Font { get; set; }

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

        protected override void LoadContent()
        {
            base.LoadContent();

            Font = Content.Load<SpriteFont>("Fonts/Consolas");
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
            var keyboardState = _keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.C) && !PrevKeyboardState.IsKeyDown(Keys.C))
            {
                // cancel all emitters in progress
                // this handles both ParticleViewer and ModelViewer
                Player.PhysicsObj.destroy_particle_manager();
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

        private static readonly Color BackgroundColor = new Color(0, 0, 0);
        
        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(BackgroundColor);

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

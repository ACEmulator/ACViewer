using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Framework.WpfInterop.Input;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;

using ACE.Server.Physics.Animation;

using ACViewer.Config;
using ACViewer.Enum;
using ACViewer.Model;
using ACViewer.Render;
using ACViewer.View;

namespace ACViewer
{
    public class ModelViewer
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public static MainWindow MainWindow => MainWindow.Instance;

        public static ModelViewer Instance { get; set; }

        public SetupInstance Setup { get; set; }
        public R_EnvCell EnvCell { get; set; }
        public R_Environment Environment { get; set; }

        public static Effect Effect => Render.Render.Effect;
        public static Effect Effect_Clamp => Render.Render.Effect_Clamp;

        public WpfKeyboard Keyboard => GameView.Instance._keyboard;
        public WpfMouse Mouse => GameView.Instance._mouse;

        public KeyboardState PrevKeyboardState => GameView.Instance.PrevKeyboardState;

        public static Camera Camera => GameView.Camera;

        public ViewObject ViewObject { get; set; }

        public bool GfxObjMode { get; set; }

        public ModelType ModelType { get; set; }

        public ModelViewer()
        {
            Instance = this;
        }

        public void LoadModel(uint id)
        {
            TextureCache.Init();

            // can be either a gfxobj or setup id
            // if gfxobj, create a simple setup
            MainWindow.Status.WriteLine($"Loading {id:X8}");
            GfxObjMode = id >> 24 == 0x01;

            Setup = new SetupInstance(id);
            InitObject(id);

            Camera.InitModel(Setup.Setup.BoundingBox);

            ModelType = ModelType.Setup;
        }

        /// <summary>
        /// Load a model with a ClothingTable
        /// </summary>
        public void LoadModel(uint setupID, ClothingTable clothingBase, PaletteTemplate paletteTemplate, float shade)
        {
            TextureCache.Init();

            // assumed to be in Setup mode for ClothingBase
            GfxObjMode = false;

            // create the ObjDesc, describing any changes to palettes / textures / gfxobj parts
            var objDesc = new Model.ObjDesc(setupID, clothingBase.Id, paletteTemplate, shade);

            Setup = new SetupInstance(setupID, objDesc);

            if (ViewObject == null || ViewObject.PhysicsObj.PartArray.Setup._dat.Id != setupID)
            {
                InitObject(setupID);

                Camera.InitModel(Setup.Setup.BoundingBox);
            }

            ModelType = ModelType.Setup;

            MainWindow.Status.WriteLine($"Loading {setupID:X8} with ClothingBase {clothingBase.Id:X8}, PaletteTemplate {paletteTemplate}, and Shade {shade}");
        }

        public void LoadEnvironment(uint envID)
        {
            ViewObject = null;
            Environment = new R_Environment(envID);

            ModelType = ModelType.Environment;
        }

        public void LoadEnvCell(uint envCellID)
        {
            // TODO: this should be more like WorldViewer, with support for various StaticObjects and their PhysicsEffects in the EnvCell

            var envCell = new ACE.Server.Physics.Common.EnvCell(DatManager.CellDat.ReadFromDat<EnvCell>(envCellID));
            envCell.Pos = new ACE.Server.Physics.Common.Position();
            EnvCell = new R_EnvCell(envCell);

            ModelType = ModelType.EnvCell;
        }

        public void LoadScript(uint scriptID)
        {
            var createParticleHooks = ParticleViewer.Instance.GetCreateParticleHooks(scriptID, 1.0f);

            ViewObject.PhysicsObj.destroy_particle_manager();
            
            foreach (var createParticleHook in createParticleHooks)
            {
                ViewObject.PhysicsObj.create_particle_emitter(createParticleHook.EmitterInfoId, (int)createParticleHook.PartIndex, new AFrame(createParticleHook.Offset), (int)createParticleHook.EmitterId);
            }
        }

        public void InitObject(uint setupID)
        {
            ViewObject = new ViewObject(setupID);

            if (Setup.Setup._setup.DefaultScript != 0)
                LoadScript(Setup.Setup._setup.DefaultScript);
        }

        public void DoStance(MotionStance stance)
        {
            if (ViewObject != null)
                ViewObject.DoStance(stance);
        }

        public void DoMotion(MotionCommand motionCommand)
        {
            if (ViewObject != null)
                ViewObject.DoMotion(motionCommand);
        }

        public void Update(GameTime time)
        {
            if (ViewObject != null)
                ViewObject.Update(time);

            if (Camera != null)
                Camera.Update(time);

            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.OemPeriod) && !PrevKeyboardState.IsKeyDown(Keys.OemPeriod))
            {
                PolyIdx++;
                Console.WriteLine($"PolyIdx: {PolyIdx}");
            }
            if (keyboardState.IsKeyDown(Keys.OemComma) && !PrevKeyboardState.IsKeyDown(Keys.OemComma))
            {
                PolyIdx--;
                Console.WriteLine($"PolyIdx: {PolyIdx}");
            }

            if (keyboardState.IsKeyDown(Keys.OemQuestion) && !PrevKeyboardState.IsKeyDown(Keys.OemQuestion))
            {
                PartIdx++;
                Console.WriteLine($"PartIdx: {PartIdx}");
            }
            if (keyboardState.IsKeyDown(Keys.M) && !PrevKeyboardState.IsKeyDown(Keys.M))
            {
                PartIdx--;
                Console.WriteLine($"PartIdx: {PartIdx}");
            }
        }

        public void Draw(GameTime time)
        {
            Effect.CurrentTechnique = Effect.Techniques["TexturedNoShading"];
            Effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            Effect.Parameters["xView"].SetValue(Camera.ViewMatrix);
            Effect.Parameters["xProjection"].SetValue(Camera.ProjectionMatrix);
            Effect.Parameters["xOpacity"].SetValue(1.0f);

            Effect_Clamp.CurrentTechnique = Effect_Clamp.Techniques["TexturedNoShading"];
            Effect_Clamp.Parameters["xWorld"].SetValue(Matrix.Identity);
            Effect_Clamp.Parameters["xView"].SetValue(Camera.ViewMatrix);
            Effect_Clamp.Parameters["xProjection"].SetValue(Camera.ProjectionMatrix);
            Effect_Clamp.Parameters["xOpacity"].SetValue(1.0f);

            switch (ModelType)
            {
                case ModelType.Setup:
                    DrawModel();
                    break;
                case ModelType.Environment:
                    DrawEnvironment();
                    break;
                case ModelType.EnvCell:
                    DrawEnvCell();
                    break;
            }

            if (MainMenu.ShowHUD)
                GameView.Instance.Render.DrawHUD();
        }

        // for debugging
        public static int PolyIdx { get; set; } = -1;

        public static int PartIdx { get; set; } = -1;

        public void DrawModel()
        {
            if (Setup == null) return;

            GraphicsDevice.Clear(ConfigManager.Config.BackgroundColors.ModelViewer);

            Setup.Draw(PolyIdx, PartIdx);

            if (ViewObject.PhysicsObj.ParticleManager != null)
                ParticleViewer.Instance.DrawParticles(ViewObject.PhysicsObj);
        }

        public void DrawEnvironment()
        {
            Effect.CurrentTechnique = Effect.Techniques["ColoredNoShading"];
            GraphicsDevice.Clear(new Color(48, 48, 48));

            if (Environment != null)
                Environment.Draw();
        }

        public void DrawEnvCell()
        {
            if (EnvCell != null)
                EnvCell.Draw(Matrix.Identity);
        }
    }
}


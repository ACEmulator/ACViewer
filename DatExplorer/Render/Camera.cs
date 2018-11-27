using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop.Input;
using DatExplorer.Model;
using DatExplorer.Render;

namespace DatExplorer
{
    public class Camera
    {
        public GameView Game;

        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;

        public Vector3 Position;
        public Vector3 Dir;
        public Vector3 Up;

        public WpfKeyboard Keyboard;
        public WpfMouse Mouse;
        public MouseState PrevMouseState;
        public int PrevScrollWheelValue;

        public float Speed { get; set; } = Model_Speed;
        public float SpeedMod = 1.5f;

        public static float NearPlane_Model = 0.1f;
        public static float NearPlane_World = 1.0f;

        public float NearPlane = 1.0f;
        public int DrawDistance = 10000;

        public float FieldOfView = 90.0f;

        public int ViewportWidth;
        public int ViewportHeight;

        public static float Model_Speed = 0.1f;
        public static float World_Speed = 0.75f;

        public Camera(GameView game)
        {
            Game = game;
            Init();
        }

        public void Init()
        {
            InitParticle();

            CreateProjection();

            Keyboard = GameView.Instance._keyboard;
            Mouse = GameView.Instance._mouse;

            //SetMouse();
        }

        public static float ParticleDist = 10.0f;

        public void InitParticle()
        {
            Position = new Vector3(ParticleDist, ParticleDist, 1);
            Dir = Vector3.Normalize(new Vector3(-ParticleDist, -ParticleDist, 0));

            Up = Vector3.UnitZ;

            CreateLookAt();
        }

        public void SetNearPlane(float nearPlane)
        {
            NearPlane = nearPlane;
            CreateProjection();
            Render.Render.Effect.Parameters["xProjection"].SetValue(ProjectionMatrix);
        }

        public void InitLandblock(R_Landblock landblock)
        {
            var x = landblock.Landblock.ID >> 24;
            var y = landblock.Landblock.ID >> 16 & 0xFF;

            var height = landblock.Vertices != null ? landblock.Vertices[0].Position.Z : 0;

            Position = new Vector3(x * 192.0f, y * 192.0f, height + 50.0f);

            var lookAt = new Vector3(x * 192.0f + 96.0f, y * 192.0f + 96.0f, height);

            Dir = Vector3.Normalize(lookAt - Position);

            Up = Vector3.UnitZ;

            Speed = World_Speed;

            CreateLookAt();
        }

        public void InitModel(Model.BoundingBox box)
        {
            //var box = setup.BoundingBox;
            var size = box.Size;

            var facing = box.GetTargetFace();
            var face = box.Faces[(int)facing];
            var center = face.Center;
            //Console.WriteLine("TargetFace: " + facing);

            var largest = Math.Max(face.Width, face.Height);
            largest = Math.Max(largest, 1.2f);
            var isAdjustWidth = face.Width > face.Height;
            var adjustWidth = face.Width / face.Height;
            var ratio = 1280.0f / 720.0f;

            Position = new Vector3(center.X, center.Y, center.Z);

            var factor = 1.414f;
            //var factor = 1.2f;

            //Console.WriteLine($"Width: {face.Width}, Height: {face.Height}");

            var gfxObjMode = ModelViewer.Instance.GfxObjMode;

            if (facing == Facing.Front || facing == Facing.Back)
            {
                if (gfxObjMode)
                    Position.Y -= largest * factor;
                else
                    Position.Y += largest * factor;

                //Position.Z *= factor;

                Up = Vector3.UnitZ;
            }
            else if (facing == Facing.Left || facing == Facing.Right)
            {
                if (gfxObjMode)
                    Position.X -= largest * factor;
                else
                    Position.X += largest * factor;

                Up = Vector3.UnitZ;
            }
            else if (facing == Facing.Top)
            {
                var front = box.Faces[(int)Facing.Front];
                var left = box.Faces[(int)Facing.Left];

                //Console.WriteLine("Front area: " + front.Area);
                //Console.WriteLine("Left area: " + left.Area);
                //Console.WriteLine("Top area: " + face.Area);

                Position.Z += largest * factor;

                Up = -Vector3.UnitY;
            }

            //Console.WriteLine("Camera pos: " + Position);

            var lookAt = box.Center;
            //lookAt.Z += size.Z * 0.1f;

            Dir = Vector3.Normalize(lookAt - Position);

            Speed = Model_Speed;

            CreateLookAt();
        }

        public void OnResize()
        {
            CreateProjection();
            Render.Render.Effect.Parameters["xProjection"].SetValue(ProjectionMatrix);
        }

        public Matrix CreateLookAt()
        {
            if (ViewportWidth != Game.GraphicsDevice.Viewport.Width ||
                ViewportHeight != Game.GraphicsDevice.Viewport.Height)
            {
                OnResize();
            }

            return ViewMatrix = Matrix.CreateLookAt(Position, Position + Dir, Up);
        }

        public Matrix CreateProjection()
        {
            ViewportWidth = Game.GraphicsDevice.Viewport.Width;
            ViewportHeight = Game.GraphicsDevice.Viewport.Height;

            var aspectRatio = (float)ViewportWidth / ViewportHeight;

            return ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                 FieldOfView * 0.0174533f / 2,       // degrees to radians
                 aspectRatio,
                 NearPlane,
                 DrawDistance);
        }

        public void Update(GameTime gameTime)
        {
            if (Mouse == null)
                return;

            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            if (!Game.IsActive)
            {
                PrevMouseState = mouseState;
                return;
            }
            if (keyboardState.IsKeyDown(Keys.W))
                Position += Dir * Speed;
            if (keyboardState.IsKeyDown(Keys.S))
                Position -= Dir * Speed;
            if (keyboardState.IsKeyDown(Keys.A))
                Position += Vector3.Cross(Up, Dir) * Speed;
            if (keyboardState.IsKeyDown(Keys.D))
                Position -= Vector3.Cross(Up, Dir) * Speed;
            if (keyboardState.IsKeyDown(Keys.Space))
                Position += Up * Speed;

            // camera speed control
            if (mouseState.ScrollWheelValue != PrevScrollWheelValue)
            {
                var diff = mouseState.ScrollWheelValue - PrevScrollWheelValue;
                if (diff >= 0)
                    Speed *= SpeedMod;
                else
                    Speed /= SpeedMod;

                PrevScrollWheelValue = mouseState.ScrollWheelValue;
            }

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                // yaw / x-rotation
                Dir = Vector3.Transform(Dir, Matrix.CreateFromAxisAngle(Up,
                    -MathHelper.PiOver4 / 160 * (mouseState.X - PrevMouseState.X)));

                // pitch / y-rotation
                Dir = Vector3.Transform(Dir, Matrix.CreateFromAxisAngle(Vector3.Cross(Up, Dir),
                    MathHelper.PiOver4 / 160 * (mouseState.Y - PrevMouseState.Y)));

                //SetMouse();

            }

            PrevMouseState = mouseState;

            Dir.Normalize();

            CreateLookAt();

            //Console.WriteLine("Camera pos: " + GameView.Instance.Render.Camera.Position);
            //Console.WriteLine("Camera dir: " + GameView.Instance.Render.Camera.Dir);
        }

        public void SetMouse()
        {
            // set mouse position to center of window
            //Mouse.SetPosition(GameView.Instance.GraphicsDevice.Viewport.Width / 2, GameView.Instance.GraphicsDevice.Viewport.Height / 2);
            //Mouse.SetCursor((int)StartPos.X, (int)StartPos.Y);
            //PrevMouseState = Mouse.GetState();
        }
    }
}

using System;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop.Input;

using ACViewer.Model;
using ACViewer.Render;

using ACE.Server.Physics.Common;

namespace ACViewer
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
        public int DrawDistance = 60000;

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

        public void InitDungeon(R_Landblock landblock, Model.BoundingBox box)
        {
            var size = box.Size;

            var center = box.Center;

            // draw a view plane from NW to SE
            var p1 = new Vector3(box.Mins.X, box.Maxs.Y, center.Z);
            var p2 = new Vector3(box.Maxs.X, box.Mins.Y, center.Z);

            var length = Vector3.Distance(p1, p2);

            var unit_length = (float)Math.Sqrt(length * length / 2);

            var dist_scalar = 0.75f;
            unit_length *= dist_scalar;

            // move camera out in -x, -y by this distance
            Position = center;
            Position.X -= unit_length;
            Position.Y -= unit_length;
            Position.Z += unit_length;

            Dir = Vector3.Normalize(box.Center - Position);

            Up = Vector3.UnitZ;
            Speed = World_Speed;

            CreateLookAt();
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
            //var ratio = 1280.0f / 720.0f;

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

        public string GetPosition()
        {
            // 255 landblocks across * 192 meters for each landblock = 48,960 meters across Dereth
            if (Position.X < 0.0f || Position.Y < 0.0f || Position.X > 48960.0f || Position.Y > 48960.0f)
                return null;
            
            var lbx = (int)(Position.X / 192.0f);
            var lby = (int)(Position.Y / 192.0f);

            var x = Position.X % 192.0f;
            var y = Position.Y % 192.0f;

            var cellX = (int)(x / 24.0f);
            var cellY = (int)(y / 24.0f);

            var cell = cellX * 8 + cellY + 1;
            
            var objCellId = (uint)(lbx << 24 | lby << 16 | cell);

            var yaw = Math.Atan2(-Dir.X, Dir.Y);

            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)yaw);

            // test if we are in any loaded indoor cells
            foreach (var landblock in LScape.Landblocks.Values)
            {
                // find origin in terms of this landblock
                var blockX = landblock.ID >> 24;
                var blockY = landblock.ID >> 16 & 0xFF;

                var blockX_start = blockX * 192.0f;
                var blockY_start = blockY * 192.0f;

                var blockPosX = Position.X - blockX_start;
                var blockPosY = Position.Y - blockY_start;

                var origin = new System.Numerics.Vector3(blockPosX, blockPosY, Position.Z);

                var envCells = landblock.get_envcells();

                foreach (var envCell in envCells)
                {
                    if (envCell.point_in_cell(origin))
                        return $"0x{envCell.ID:X8} [{blockPosX} {blockPosY} {Position.Z}] {q.W} {q.X} {q.Y} {q.Z}";
                }
            }

            // return outdoor location
            return $"0x{objCellId:X8} [{x} {y} {Position.Z}] {q.W} {q.X} {q.Y} {q.Z}";
        }
    }
}

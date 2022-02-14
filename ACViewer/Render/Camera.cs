using System;
using System.Windows.Input;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using MonoGame.Framework.WpfInterop.Input;

using ACE.Server.Physics.Animation;
using ACE.Server.Physics.Common;

using ACViewer.Config;
using ACViewer.Enum;
using ACViewer.Extensions;
using ACViewer.Render;
using ACViewer.View;

namespace ACViewer
{
    public class Camera
    {
        public GameView GameView { get; set; }

        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Dir { get; set; }
        public Vector3 Up { get; set; }

        public WpfKeyboard Keyboard => GameView._keyboard;
        public WpfMouse Mouse => GameView._mouse;

        public MouseState PrevMouseState => GameView.PrevMouseState;

        public float Speed { get; set; } = Model_Speed;

        public float SpeedMod { get; set; } = 1.5f;

        public static float NearPlane_Model { get; set; } = 0.1f;
        public static float NearPlane_World { get; set; } = 1.0f;

        public float NearPlane { get; set; } = 1.0f;

        public int DrawDistance { get; set; } = 60000;

        public float FieldOfView { get; set; } = 90.0f;

        public int ViewportWidth { get; set; }
        public int ViewportHeight { get; set; }

        public static float Model_Speed { get; set; } = 0.1f;
        public static float World_Speed { get; set; } = 0.75f;

        public Camera(GameView game)
        {
            GameView = game;
            
            Init();
        }

        public void Init()
        {
            InitParticle();

            CreateProjection();

            //SetMouse();
        }

        public static readonly float ParticleDist = 10.0f;

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
            Render.Render.Effect_Clamp.Parameters["xProjection"].SetValue(ProjectionMatrix);
        }

        public void InitLandblock(R_Landblock landblock)
        {
            var x = landblock.Landblock.ID >> 24;
            var y = landblock.Landblock.ID >> 16 & 0xFF;

            var height = landblock.Landblock.Polygons[0].Vertices[0].Origin.Z;
            
            Position = new Vector3(x * 192.0f, y * 192.0f, height + 50.0f);

            var lookAt = new Vector3(x * 192.0f + 96.0f, y * 192.0f + 96.0f, height);

            Dir = Vector3.Normalize(lookAt - Position);

            Up = Vector3.UnitZ;

            Speed = World_Speed;

            CreateLookAt();
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
            var position = center;
            position.X -= unit_length;
            position.Y -= unit_length;
            position.Z += unit_length;

            Position = position;

            Dir = Vector3.Normalize(box.Center - Position);

            Up = Vector3.UnitZ;
            Speed = World_Speed;

            CreateLookAt();
        }

        public void InitTeleport(R_Landblock landblock, float zBump)
        {
            var origin = Teleport.Origin;
            var orientation = Teleport.Orientation;

            var lbx = landblock.Landblock.ID >> 24;
            var lby = landblock.Landblock.ID >> 16 & 0xFF;

            Position = new Vector3(lbx * 192.0f + origin.X, lby * 192.0f + origin.Y, origin.Z + zBump);

            Dir = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, Matrix.CreateFromQuaternion(orientation.ToXna())));

            Up = Vector3.UnitZ;

            Speed = World_Speed;

            CreateLookAt();
        }

        public void InitModel(Model.BoundingBox box)
        {
            //var box = setup.BoundingBox;
            var size = box.Size;

            if (float.IsInfinity(size.X))
            {
                InitParticle();
                return;
            }

            var facing = box.GetTargetFace();
            var face = box.Faces[(int)facing];
            var center = face.Center;

            //Console.WriteLine("TargetFace: " + facing);

            var largest = Math.Max(face.Width, face.Height);
            largest = Math.Max(largest, 1.2f);

            var position = new Vector3(center.X, center.Y, center.Z);

            var factor = 1.414f;
            //var factor = 1.2f;

            //Console.WriteLine($"Width: {face.Width}, Height: {face.Height}");

            var gfxObjMode = ModelViewer.Instance.GfxObjMode;
            
            if (facing == Facing.Front || facing == Facing.Back)
            {
                if (gfxObjMode)
                    position.Y -= largest * factor;
                else
                    position.Y += largest * factor;

                //position.Z *= factor;

                Up = Vector3.UnitZ;
            }
            else if (facing == Facing.Left || facing == Facing.Right)
            {
                if (gfxObjMode)
                    position.X -= largest * factor;
                else
                    position.X += largest * factor;

                Up = Vector3.UnitZ;
            }
            else if (facing == Facing.Top)
            {
                var front = box.Faces[(int)Facing.Front];
                var left = box.Faces[(int)Facing.Left];

                //Console.WriteLine("Front area: " + front.Area);
                //Console.WriteLine("Left area: " + left.Area);
                //Console.WriteLine("Top area: " + face.Area);

                position.Z += largest * factor;

                Up = -Vector3.UnitY;
            }

            Position = position;

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
            Render.Render.Effect_Clamp.Parameters["xProjection"].SetValue(ProjectionMatrix);
        }

        public Matrix CreateLookAt()
        {
            if (ViewportWidth != GameView.GraphicsDevice.Viewport.Width ||
                ViewportHeight != GameView.GraphicsDevice.Viewport.Height)
            {
                OnResize();
            }

            return ViewMatrix = Matrix.CreateLookAt(Position, Position + Dir, Up);
        }

        public Matrix CreateProjection()
        {
            ViewportWidth = GameView.GraphicsDevice.Viewport.Width;
            ViewportHeight = GameView.GraphicsDevice.Viewport.Height;

            var aspectRatio = (float)ViewportWidth / ViewportHeight;

            return ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                 FieldOfView * 0.0174533f / 2,       // degrees to radians
                 aspectRatio,
                 NearPlane,
                 DrawDistance);
        }

        private static readonly float SpeedBase = MathHelper.PiOver4 / 160 / 6;

        public bool Locked { get; set; }

        public System.Windows.Point LastSetPoint { get; set; }

        public void Update(GameTime gameTime)
        {
            if (Mouse == null || Locked) return;

            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            if (!GameView.IsActive) return;

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
            if (mouseState.ScrollWheelValue != PrevMouseState.ScrollWheelValue)
            {
                var diff = mouseState.ScrollWheelValue - PrevMouseState.ScrollWheelValue;

                if (diff >= 0)
                    Speed *= SpeedMod;
                else
                    Speed /= SpeedMod;
            }

            if (mouseState.LeftButton == ButtonState.Pressed && PrevMouseState.LeftButton != ButtonState.Pressed)
            {
                if (GameView.ViewMode == ViewMode.World)
                    Picker.HandleLeftClick(mouseState.X, mouseState.Y);
            }

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                if (PrevMouseState.RightButton == ButtonState.Pressed)
                {
                    MouseEx.GetCursorPos(out var cursorPos);
                    
                    var xDiff = cursorPos.X - (int)LastSetPoint.X;
                    var yDiff = cursorPos.Y - (int)LastSetPoint.Y;

                    if (ConfigManager.Config.Mouse.AltMethod)
                    {
                        xDiff = mouseState.X - PrevMouseState.X;
                        yDiff = mouseState.Y - PrevMouseState.Y;
                    }

                    // yaw / x-rotation
                    Dir = Vector3.Transform(Dir, Matrix.CreateFromAxisAngle(Up,
                        -SpeedBase * ConfigManager.Config.Mouse.Speed * xDiff));

                    // pitch / y-rotation
                    Dir = Vector3.Transform(Dir, Matrix.CreateFromAxisAngle(Vector3.Cross(Up, Dir),
                        SpeedBase * ConfigManager.Config.Mouse.Speed * yDiff));

                    if (MainWindow.DebugMode && (xDiff != 0 || yDiff != 0))
                    {
                        if (!ConfigManager.Config.Mouse.AltMethod)
                            Console.WriteLine($"mouseX: {mouseState.X}, mouseY: {mouseState.Y}, centerX: {centerX}, centerY: {centerY}");
                        else
                            Console.WriteLine($"xDiff: {xDiff}, yDiff: {yDiff}");
                    }
                }
                else
                {
                    System.Windows.Input.Mouse.OverrideCursor = Cursors.None;
                }
                // there is a delay here, so Mouse.GetState() won't be immediately affected
                if (!ConfigManager.Config.Mouse.AltMethod)
                    LastSetPoint = MouseEx.SetCursor(GameView.Instance, centerX, centerY);
            }
            else if (PrevMouseState.RightButton == ButtonState.Pressed)
            {
                if (ConfigManager.Config.Mouse.AltMethod)
                    Mouse.SetCursor(centerX, centerY);

                System.Windows.Input.Mouse.OverrideCursor = null;
            }

            Dir.Normalize();

            CreateLookAt();

            //Console.WriteLine("Camera pos: " + GameView.Instance.Render.Camera.Position);
            //Console.WriteLine("Camera dir: " + GameView.Instance.Render.Camera.Dir);
        }

        public int centerX => GameView.GraphicsDevice.Viewport.Width / 2;
        public int centerY => GameView.GraphicsDevice.Viewport.Height / 2;

        public Position GetPosition()
        {
            // 255 landblocks across * 192 meters for each landblock = 48,960 meters across Dereth
            if (GameView.ViewMode == ViewMode.World && (Position.X < 0.0f || Position.Y < 0.0f || Position.X > 48960.0f || Position.Y > 48960.0f))
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
                        //return $"0x{envCell.ID:X8} [{blockPosX} {blockPosY} {Position.Z}] {q.W} {q.X} {q.Y} {q.Z}";
                        return new Position(envCell.ID, new AFrame(new System.Numerics.Vector3(blockPosX, blockPosY, Position.Z), q.ToNumerics()));
                }
            }

            // return outdoor location
            //return $"0x{objCellId:X8} [{x} {y} {Position.Z}] {q.W} {q.X} {q.Y} {q.Z}";
            return new Position(objCellId, new AFrame(new System.Numerics.Vector3(x, y, Position.Z), q.ToNumerics()));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using MonoGame.Framework.WpfInterop.Input;

using ACE.Entity.Enum;
using ACE.Server.Physics;
using ACE.Server.Physics.Animation;
using ACE.Server.Physics.Common;
using ACE.Server.WorldObjects;

using ACViewer.Config;
using ACViewer.Extensions;
using ACViewer.Model;
using ACViewer.View;

namespace ACViewer
{
    public class Player
    {
        public static Camera Camera => GameView.Camera;

        public static uint NextPlayerGuid = 0x50000001;
        
        public ACE.Server.WorldObjects.Player WorldObject { get; set; }

        public PhysicsObj PhysicsObj => WorldObject.PhysicsObj;

        public bool FullSim { get; set; }

        public WpfKeyboard Keyboard => GameView.Instance._keyboard;
        public WpfMouse Mouse => GameView.Instance._mouse;

        public KeyboardState PrevKeyboardState => GameView.Instance.PrevKeyboardState;

        public MouseState PrevMouseState => GameView.Instance.PrevMouseState;

        public Player(bool fullSim = false)
        {
            FullSim = fullSim;

            WorldObject = new ACE.Server.WorldObjects.Player();
            WorldObject.Name = "Player";
            //worldObj.RunSkill = runSkill;
            //worldObj.IsCreature = true;

            WorldObject.Strength.StartingValue = 400;
            WorldObject.Endurance.StartingValue = 400;
            WorldObject.Coordination.StartingValue = 400;
            WorldObject.Quickness.StartingValue = 400;
            WorldObject.Focus.StartingValue = 400;
            WorldObject.Self.StartingValue = 400;

            var run = WorldObject.GetCreatureSkill(Skill.Run);
            run.InitLevel = 100;

            var jump = WorldObject.GetCreatureSkill(Skill.Jump);
            jump.InitLevel = 450;

            WorldObject.PhysicsObj = new PhysicsObj();
            PhysicsObj.set_object_guid(new ACE.Entity.ObjectGuid(NextPlayerGuid++));

            // player
            uint modelID = 0x02000001;
            uint mTableID = 0x09000001;
            //uint runSkill = 300;
            float scale = 1.0f;

            PhysicsObj.makeAnimObject(modelID, true);

            var weenie = new WeenieObject(WorldObject);
            PhysicsObj.set_weenie_obj(weenie);

            PhysicsObj.SetMotionTableID(mTableID);
            PhysicsObj.SetScaleStatic(scale);

            if (!fullSim)
                PhysicsObj.ParticleManager = new ParticleManager();

            RawMotionState = new RawMotionState();
            RawMotionState.CurrentHoldKey = HoldKey.Run;
        }

        public static readonly float Speed = 4.0f;

        public static readonly List<Keys> ForwardKeys = new List<Keys>()
        {
            Keys.W,
            Keys.Up,
            Keys.NumPad8,
        };

        public static readonly List<Keys> BackwardKeys = new List<Keys>()
        {
            Keys.X,
            Keys.Down,
            Keys.NumPad2,
        };

        public static readonly List<Keys> TurnLeftKeys = new List<Keys>()
        {
            Keys.A,
            Keys.Left,
            Keys.NumPad4,
        };

        public static readonly List<Keys> TurnRightKeys = new List<Keys>()
        {
            Keys.D,
            Keys.Right,
            Keys.NumPad6,
        };

        public static readonly List<Keys> StrafeLeftKeys = new List<Keys>()
        {
            Keys.Z,
            Keys.NumPad1,
        };

        public static readonly List<Keys> StrafeRightKeys = new List<Keys>()
        {
            Keys.C,
            Keys.NumPad3,
        };

        public static readonly List<Keys> WalkKeys = new List<Keys>()
        {
            Keys.LeftShift,
        };

        public static readonly List<Keys> JumpKeys = new List<Keys>()
        {
            Keys.Space,
        };

        public static bool GetKeyState(KeyboardState keyboardState, List<Keys> keys)
        {
            foreach (var key in keys)
            {
                if (keyboardState.IsKeyDown(key))
                    return true;
            }
            return false;
        }

        private static readonly float MouseSpeedBase = 0.5f / 6.0f;

        public System.Windows.Point LastSetPoint { get; set; }

        public void Update(GameTime time)
        {
            if (!FullSim)
            {
                UpdatePhysics(time);
                return;
            }

            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            var isForward = GetKeyState(keyboardState, ForwardKeys);
            var isBackward = GetKeyState(keyboardState, BackwardKeys);
            var isTurnLeft = GetKeyState(keyboardState, TurnLeftKeys);
            var isTurnRight = GetKeyState(keyboardState, TurnRightKeys);
            var isStrafeLeft = GetKeyState(keyboardState, StrafeLeftKeys);
            var isStrafeRight = GetKeyState(keyboardState, StrafeRightKeys);
            var isWalk = GetKeyState(keyboardState, WalkKeys);
            var isJump = GetKeyState(keyboardState, JumpKeys);

            var wasForward = GetKeyState(PrevKeyboardState, ForwardKeys);
            var wasBackward = GetKeyState(PrevKeyboardState, BackwardKeys);
            var wasTurnLeft = GetKeyState(PrevKeyboardState, TurnLeftKeys);
            var wasTurnRight = GetKeyState(PrevKeyboardState, TurnRightKeys);
            var wasStrafeLeft = GetKeyState(PrevKeyboardState, StrafeLeftKeys);
            var wasStrafeRight = GetKeyState(PrevKeyboardState, StrafeRightKeys);
            var wasWalk = GetKeyState(PrevKeyboardState, WalkKeys);
            var wasJump = GetKeyState(PrevKeyboardState, JumpKeys);

            var isLeftClick = mouseState.LeftButton == ButtonState.Pressed;
            var wasLeftClick = PrevMouseState.LeftButton == ButtonState.Pressed;

            var isRightClick = mouseState.RightButton == ButtonState.Pressed;
            var wasRightClick = PrevMouseState.RightButton == ButtonState.Pressed;

            if (isForward && !wasForward)
            {
                RunForward(Speed, true);
            }

            if (!isForward && wasForward)
            {
                RunForward(Speed, false);
            }

            if (isBackward && !wasBackward)
            {
                WalkBackward(Speed);
            }

            if (!isBackward && wasBackward)
            {
                WalkBackward(Speed, false);
            }

            if (isTurnLeft && !wasTurnLeft /*|| isMouseLookLeft && !wasMouseLookLeft*/)
            {
                TurnLeft();
            }

            if (!isTurnLeft && wasTurnLeft /*|| !isMouseLookLeft && !isMouseLookRight && wasMouseLookLeft*/)
            {
                TurnLeft(false);
            }

            if (isTurnRight && !wasTurnRight /* || isMouseLookRight && !wasMouseLookRight*/)
            {
                TurnRight();
            }

            if (!isTurnRight && wasTurnRight /* || !isMouseLookRight && !isMouseLookLeft && wasMouseLookRight*/)
            {
                TurnRight(false);
            }

            if (isStrafeLeft && !wasStrafeLeft)
            {
                StrafeLeft();
            }

            if (!isStrafeLeft && wasStrafeLeft)
            {
                StrafeLeft(false);
            }

            if (isStrafeRight && !wasStrafeRight)
            {
                StrafeRight();
            }

            if (!isStrafeRight && wasStrafeRight)
            {
                StrafeRight(false);
            }

            /*if (keyboardState.IsKeyDown(Keys.OemOpenBrackets) && !PrevKeyboardState.IsKeyDown(Keys.OemOpenBrackets))
            {
                CurrentHeightFactor -= 0.025f;

                Console.WriteLine($"CurrentHeightFactor: {CurrentHeightFactor}");
            }

            if (keyboardState.IsKeyDown(Keys.OemCloseBrackets) && !PrevKeyboardState.IsKeyDown(Keys.OemCloseBrackets))
            {
                CurrentHeightFactor += 0.025f;

                Console.WriteLine($"CurrentHeightFactor: {CurrentHeightFactor}");
            }*/

            if (keyboardState.IsKeyDown(Keys.I) && !PrevKeyboardState.IsKeyDown(Keys.I))
            {
                var ethereal = PhysicsObj.State.HasFlag(PhysicsState.Ethereal);

                ethereal = !ethereal;

                PhysicsObj.set_ethereal(ethereal, true);

                MainWindow.Instance.AddStatusText($"Ethereal: {ethereal}");
            }

            if (isWalk && !wasWalk)
            {
                SetHoldKey(HoldKey.None);
                ApplyRawState();
            }

            if (!isWalk && wasWalk)
            {
                SetHoldKey(HoldKey.Run);
                ApplyRawState();
            }

            if (isRightClick)
            {
                if (wasRightClick)
                {
                    MouseEx.GetCursorPos(out var cursorPos);

                    var diffX = cursorPos.X - (int)LastSetPoint.X;
                    var diffY = cursorPos.Y - (int)LastSetPoint.Y;

                    if (ConfigManager.Config.Mouse.AltMethod)
                    {
                        diffX = mouseState.X - PrevMouseState.X;
                        diffY = mouseState.Y - PrevMouseState.Y;
                    }

                    if (diffX != 0)
                    {
                        var heading = PhysicsObj.get_heading();
                        heading += diffX * MouseSpeedBase * ConfigManager.Config.Mouse.Speed;
                        PhysicsObj.set_heading(heading, false);
                    }

                    if (diffY > 0)
                    {
                        CurrentAngle -= diffY * MouseSpeedBase * ConfigManager.Config.Mouse.Speed;

                        if (CurrentAngle < MaxAngleDown)
                            CurrentAngle = MaxAngleDown;

                        //Console.WriteLine($"CurrentAngle: {CurrentAngle}");
                    }
                    else if (diffY < 0)
                    {
                        CurrentAngle -= diffY * MouseSpeedBase * ConfigManager.Config.Mouse.Speed;

                        if (CurrentAngle > MaxAngleUp)
                            CurrentAngle = MaxAngleUp;

                        //Console.WriteLine($"CurrentAngle: {CurrentAngle}");
                    }
                }
                else
                {
                    System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.None;
                }

                if (!ConfigManager.Config.Mouse.AltMethod)
                    LastSetPoint = MouseEx.SetCursor(GameView.Instance, Camera.centerX, Camera.centerY);
            }
            else if (wasRightClick)
            {
                if (ConfigManager.Config.Mouse.AltMethod)
                    Mouse.SetCursor(Camera.centerX, Camera.centerY);

                System.Windows.Input.Mouse.OverrideCursor = null;
            }

            if (mouseState.ScrollWheelValue != PrevMouseState.ScrollWheelValue)
            {
                var diff = mouseState.ScrollWheelValue - PrevMouseState.ScrollWheelValue;

                if (diff >= 0)
                    CurrentDist -= 0.5f;
                else
                    CurrentDist += 0.5f;

                //Console.WriteLine($"CurrentDist: {CurrentDist}");
            }

            UpdatePhysics(time);

            if (isLeftClick && !wasLeftClick)
            {
                Picker.HandleLeftClick(mouseState.X, mouseState.Y);
            }

            if (isJump && !wasJump)
            {
                // check if grounded?
                if (PhysicsObj.TransientState.HasFlag(TransientStateFlags.OnWalkable))
                {
                    JumpMeter.Start();

                    var minterp = PhysicsObj.get_minterp();
                    
                    if (PhysicsObj.TransientState.HasFlag(TransientStateFlags.Contact | TransientStateFlags.OnWalkable) && minterp.InterpretedState.ForwardCommand == (uint)MotionCommand.Ready &&
                        minterp.InterpretedState.SideStepCommand == 0 && minterp.InterpretedState.TurnCommand == 0)
                    {
                        minterp.StandingLongJump = true;
                    }
                }
            }

            if (!isJump && wasJump)
            {
                JumpMeter.Stop();

                // check if grounded?
                if (PhysicsObj.TransientState.HasFlag(TransientStateFlags.OnWalkable))
                {
                    // calc velocity
                    PhysicsObj.WeenieObj.InqJumpVelocity(JumpMeter.Percent, out var jumpVelocityZ);

                    var minterp = PhysicsObj.get_minterp();

                    var jumpVelocity = minterp.get_leave_ground_velocity();
                    jumpVelocity.Z = jumpVelocityZ;
                    
                    // perform physics jump
                    PhysicsObj.TransientState &= ~(TransientStateFlags.Contact | TransientStateFlags.WaterContact);
                    PhysicsObj.calc_acceleration();
                    PhysicsObj.set_on_walkable(false);
                    PhysicsObj.set_local_velocity(jumpVelocity, false);
                    minterp.StandingLongJump = false;
                }
            }

            if (JumpMeter.IsCharging)
                JumpMeter.Update();
        }

        public void BuildRawState()
        {
            var minterp = PhysicsObj.get_minterp();

            minterp.RawState = new RawMotionState(minterp.RawState, RawMotionState);
        }
        
        public void ApplyRawState()
        {
            BuildRawState();
            
            var minterp = PhysicsObj.get_minterp();

            var allowJump = minterp.motion_allows_jump(minterp.InterpretedState.ForwardCommand) == WeenieError.None;

            minterp.apply_raw_movement(true, allowJump);
        }

        public RawMotionState RawMotionState { get; set; }

        public void SetForwardCommand(MotionCommand motion)
        {
            RawMotionState.ForwardCommand = (uint)motion;
        }

        public void SetTurnCommand(MotionCommand motion)
        {
            RawMotionState.TurnCommand = (uint)motion;
        }

        public void SetSideStepCommand(MotionCommand motion)
        {
            RawMotionState.SideStepCommand = (uint)motion;
        }

        public void SetHoldKey(HoldKey holdKey)
        {
            RawMotionState.CurrentHoldKey = holdKey;
        }

        public void RemoveForwardCommand()
        {
            RawMotionState.ForwardCommand = (uint)MotionCommand.Ready;
        }

        public void RemoveTurnCommand()
        {
            RawMotionState.TurnCommand = 0;
        }

        public void RemoveSideStepCommand()
        {
            RawMotionState.SideStepCommand = 0;
        }

        public void RunForward(float speed = 1.0f, bool start = true)
        {
            DoMotion(MotionCommand.WalkForward, speed, start);
        }

        public void WalkBackward(float speed = 1.0f, bool start = true)
        {
            DoMotion(MotionCommand.WalkBackwards, speed, start);
        }

        public float TurnSpeed = 1.0f;

        public void TurnLeft(bool start = true)
        {
            DoMotion(MotionCommand.TurnLeft, TurnSpeed, start);
        }

        public void TurnRight(bool start = true)
        {
            DoMotion(MotionCommand.TurnRight, TurnSpeed, start);
        }

        public float StrafeSpeed = 1.0f;

        public void StrafeLeft(bool start = true)
        {
            DoMotion(MotionCommand.SideStepLeft, StrafeSpeed, start);
        }

        public void StrafeRight(bool start = true)
        {
            DoMotion(MotionCommand.SideStepRight, StrafeSpeed, start);
        }

        public void DoMotion(MotionCommand motionCommand, float speed = 1.0f, bool start = true)
        {
            var keyboardState = Keyboard.GetState();
            
            var holdKey = keyboardState.IsKeyDown(Keys.LeftShift) ? HoldKey.None : HoldKey.Run;

            var mvp = new MovementParameters();
            mvp.HoldKeyToApply = holdKey;

            SetHoldKey(holdKey);

            if (!PhysicsObj.TransientState.HasFlag(TransientStateFlags.OnWalkable))
                BuildRawState();

            // handle ForwardCommand (forward / backward)
            if (motionCommand == MotionCommand.WalkForward)
            {
                if (start)
                {
                    PhysicsObj.DoMotion((uint)MotionCommand.Ready, mvp);

                    PhysicsObj.DoMotion((uint)motionCommand, mvp);

                    SetForwardCommand(motionCommand);

                    //Console.WriteLine($"RawState.ForwardCommand: {(MotionCommand)PhysicsObj.MovementManager.MotionInterpreter.RawState.ForwardCommand}");
                    //Console.WriteLine($"InterpretedState.ForwardCommand: {(MotionCommand)PhysicsObj.MovementManager.MotionInterpreter.InterpretedState.ForwardCommand}");
                }
                else
                {
                    if (GetKeyState(keyboardState, BackwardKeys))
                    {
                        PhysicsObj.DoMotion((uint)MotionCommand.WalkBackwards, mvp);

                        SetForwardCommand(MotionCommand.WalkBackwards);
                    }
                    else
                    {
                        PhysicsObj.StopMotion((uint)motionCommand, mvp, true);

                        RemoveForwardCommand();
                    }
                }
            }

            else if (motionCommand == MotionCommand.WalkBackwards)
            {
                if (start)
                {
                    /*PhysicsObj.StopMotion((uint)MotionCommand.SideStepRight, mvp, true);
                    PhysicsObj.StopMotion((uint)MotionCommand.TurnRight, mvp, true);
                    PhysicsObj.StopMotion((uint)MotionCommand.SideStepLeft, mvp, true);*/
                    PhysicsObj.DoMotion((uint)MotionCommand.Ready, mvp);

                    PhysicsObj.DoMotion((uint)motionCommand, mvp);

                    SetForwardCommand(motionCommand);
                }
                else
                {
                    if (GetKeyState(keyboardState, ForwardKeys))
                    {
                        PhysicsObj.DoMotion((uint)MotionCommand.WalkForward, mvp);

                        SetForwardCommand(MotionCommand.WalkForward);
                    }
                    else
                    {
                        PhysicsObj.StopMotion((uint)motionCommand, mvp, true);

                        RemoveForwardCommand();
                    }
                }
            }

            // handle strafe
            else if (motionCommand == MotionCommand.SideStepLeft)
            {
                if (start)
                {
                    PhysicsObj.DoMotion((uint)motionCommand, mvp);

                    SetSideStepCommand(motionCommand);
                }
                else
                {
                    if (GetKeyState(keyboardState, StrafeRightKeys))
                    {
                        PhysicsObj.DoMotion((uint)MotionCommand.SideStepRight, mvp);

                        SetSideStepCommand(MotionCommand.SideStepRight);
                    }
                    else
                    {
                        PhysicsObj.StopMotion((uint)motionCommand, mvp, true);

                        RemoveSideStepCommand();
                    }
                }
            }

            else if (motionCommand == MotionCommand.SideStepRight)
            {
                if (start)
                {
                    PhysicsObj.DoMotion((uint)motionCommand, mvp);

                    SetSideStepCommand(motionCommand);
                }
                else
                {
                    if (GetKeyState(keyboardState, StrafeLeftKeys))
                    {
                        PhysicsObj.DoMotion((uint)MotionCommand.SideStepLeft, mvp);

                        SetSideStepCommand(MotionCommand.SideStepLeft);
                    }
                    else
                    {
                        PhysicsObj.StopMotion((uint)motionCommand, mvp, true);

                        RemoveSideStepCommand();
                    }
                }
            }

            // handle turning
            else if (motionCommand == MotionCommand.TurnLeft)
            {
                if (start)
                {
                    PhysicsObj.DoMotion((uint)motionCommand, mvp);

                    SetTurnCommand(motionCommand);
                }
                else
                {
                    if (GetKeyState(keyboardState, TurnRightKeys))
                    {
                        PhysicsObj.DoMotion((uint)MotionCommand.TurnRight, mvp);

                        SetTurnCommand(MotionCommand.TurnRight);
                    }
                    else
                    {
                        PhysicsObj.StopMotion((uint)motionCommand, mvp, true);

                        RemoveTurnCommand();
                    }
                }
            }

            else if (motionCommand == MotionCommand.TurnRight)
            {
                if (start)
                {
                    PhysicsObj.DoMotion((uint)motionCommand, mvp);

                    SetTurnCommand(motionCommand);
                }
                else
                {
                    if (GetKeyState(keyboardState, TurnLeftKeys))
                    {
                        PhysicsObj.DoMotion((uint)MotionCommand.TurnLeft, mvp);

                        SetTurnCommand(MotionCommand.TurnLeft);
                    }
                    else
                    {
                        PhysicsObj.StopMotion((uint)motionCommand, mvp, true);

                        RemoveTurnCommand();
                    }
                }
            }

            else
            {
                PhysicsObj.DoMotion((uint)motionCommand, mvp);
            }
        }

        public void UpdatePhysics(GameTime time)
        {
            if (!GameView.Instance.IsActive) return;

            if (!FullSim)
            {
                if (PhysicsObj.ParticleManager != null)
                    PhysicsObj.ParticleManager.UpdateParticles();

                return;
            }

            PhysicsObj.update_object();

            SetCameraBehind();

            // update visuals?

            //QueryParticleManager();
        }

        public void QueryParticleManager()
        {
            var particleManager = PhysicsObj.ParticleManager;

            if (particleManager == null || particleManager.ParticleTable.Count == 0)
                return;

            var emitter = particleManager.ParticleTable.Values.FirstOrDefault();
            var firstParticle = emitter.Particles[0];

            var i = 0;
            foreach (var particlePart in emitter.Parts)
            {
                if (particlePart != null && particlePart.Pos.Frame.Origin.X != 0.0f && particlePart.Pos.Frame.Origin.Y != 0.0f && particlePart.Pos.Frame.Origin.Z != 0.0f)
                {
                    Console.WriteLine($"ParticlePart[{i}] = {particlePart.Pos.Frame.Origin}");
                }
                i++;
                break;
            }
            var nonNull = emitter.Parts.Where(p => p != null);
            //Console.WriteLine("Particles: " + nonNull.Count());
        }

        public static readonly float DefaultAngle = -15.0f;

        public static readonly float MaxAngleDown = -89.0f;

        public static readonly float MaxAngleUp = 60.0f;

        public static readonly float DefaultDist = 4.15f;

        public static readonly float DefaultHeightFactor = 1.275f;

        public static float CurrentAngle { get; set; } = DefaultAngle;

        public static float CurrentDist { get; set; } = DefaultDist;

        public static float CurrentHeightFactor { get; set; } = DefaultHeightFactor;

        public void SetCameraBehind()
        {
            var rads = CurrentAngle.ToRads();

            var dir = new Vector3(0, (float)Math.Cos(rads), (float)Math.Sin(rads)) * CurrentDist;

            // transform by player rotation
            Camera.Dir = Vector3.Transform(dir, PhysicsObj.Position.Frame.Orientation.ToXna());
            Camera.Dir.Normalize();

            var lookAt = PhysicsObj.Position.GetWorldPos();
            lookAt.Z += PhysicsObj.GetHeight() / CurrentHeightFactor;

            // set camera position
            Camera.Position = lookAt - Camera.Dir;

            Camera.CreateLookAt();
        }

        public JumpMeter JumpMeter { get; set; } = new JumpMeter();

        public void Draw()
        {
            if (JumpMeter.IsCharging)
                JumpMeter.Draw();
        }
    }
}

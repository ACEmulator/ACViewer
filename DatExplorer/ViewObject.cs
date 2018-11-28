﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity.Enum;
using ACE.Server.Physics;
using ACE.Server.Physics.Animation;
using ACE.Server.Physics.Common;
using ACE.Server.WorldObjects;
using Microsoft.Xna.Framework;
using DatExplorer.Data;
using DatExplorer.Render;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

namespace DatExplorer
{
    public class ViewObject
    {
        public GameView GameView { get => GameView.Instance; }

        public PhysicsObj PhysicsObj;
        public R_Environment Environment;

        public static uint NextGuid = 1;

        public ViewObject(uint setupID)
        {
            PhysicsObj = new PhysicsObj();
            var guid = new ACE.Entity.ObjectGuid(NextGuid++);
            PhysicsObj.set_object_guid(guid);

            uint modelID = setupID;

            uint runSkill = 300;
            float scale = 1.0f;

            if (modelID >> 24 == 0x1)
                PhysicsObj = PhysicsObj.makeObject(modelID, guid.Full, false);
            else
                PhysicsObj.makeAnimObject(modelID, true);

            // fake contact / on ground
            PhysicsObj.TransientState |= TransientStateFlags.Contact | TransientStateFlags.OnWalkable;

            // fake cell
            PhysicsObj.CurCell = new ObjCell();

            var worldObj = new WorldObject();
            worldObj.Name = $"Obj {setupID:X8}";
            worldObj.RunSkill = runSkill;
            worldObj.IsCreature = true;
            var weenie = new WeenieObject(worldObj);
            PhysicsObj.set_weenie_obj(weenie);

            var didTable = DIDTables.Get(setupID);
            if (didTable != null)
            {
                var mTableID = didTable.MotionTableID;
                PhysicsObj.SetMotionTableID(mTableID);
            }

            PhysicsObj.SetScaleStatic(scale);

            PhysicsObj.set_initial_frame(new AFrame());
        }

        public void DoStance(MotionStance stance)
        {
            var rawState = new RawMotionState();
            rawState.ForwardCommand = (uint)stance;
            rawState.CurrentHoldKey = HoldKey.Run;

            var motionInterp = PhysicsObj.get_minterp();
            motionInterp.RawState = rawState;
            motionInterp.apply_raw_movement(true, true);

            //if (PhysicsObj.PartArray.MotionTableManager.PendingAnimations.Count() > 0)
                //Console.WriteLine("Motions pending");
        }

        public void DoMotion(MotionCommand motionCommand)
        {
            var motionInterp = PhysicsObj.get_minterp();

            var rawState = new RawMotionState();
            rawState.CurrentStyle = motionInterp.InterpretedState.CurrentStyle;
            rawState.CurrentHoldKey = HoldKey.Run;

            rawState.ForwardCommand = (uint)motionCommand;

            motionInterp.RawState = rawState;
            motionInterp.apply_raw_movement(true, true);

            //if (PhysicsObj.PartArray.MotionTableManager.PendingAnimations.Count() > 0)
                //Console.WriteLine("Motions pending");
        }

        public void Update(GameTime time)
        {
            UpdatePhysics(time);
        }

        public void UpdatePhysics(GameTime time)
        {
            if (!GameView.IsActive)
                return;

            // update anim only?
            PhysicsObj.update_animation();

            var minterp = PhysicsObj.get_minterp();
            //if (minterp.motions_pending())
                //Console.WriteLine("Motions pending");
        }
    }
}

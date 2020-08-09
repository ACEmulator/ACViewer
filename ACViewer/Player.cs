using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Server.Physics;
using ACE.Server.Physics.Common;
using ACE.Server.WorldObjects;
using Microsoft.Xna.Framework;

namespace ACViewer
{
    public class Player
    {
        public GameView GameView { get => GameView.Instance; }

        public PhysicsObj PhysicsObj;

        public Player()
        {
            PhysicsObj = new PhysicsObj();
            PhysicsObj.set_object_guid(new ACE.Entity.ObjectGuid(1));

            // player
            uint modelID = 0x02000001;
            uint mTableID = 0x09000001;
            uint runSkill = 300;
            float scale = 1.0f;

            PhysicsObj.makeAnimObject(modelID, true);

            var worldObj = new WorldObject();
            worldObj.Name = "Player";
            worldObj.RunSkill = runSkill;
            worldObj.IsCreature = true;
            var weenie = new WeenieObject(worldObj);
            PhysicsObj.set_weenie_obj(weenie);

            PhysicsObj.SetMotionTableID(mTableID);
            PhysicsObj.SetScaleStatic(scale);

            PhysicsObj.ParticleManager = new ParticleManager();
        }

        public void Update(GameTime time)
        {
            UpdatePhysics(time);
        }

        public void UpdatePhysics(GameTime time)
        {
            if (!GameView.IsActive)
                return;

            PhysicsObj.ParticleManager.UpdateParticles();

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
    }
}

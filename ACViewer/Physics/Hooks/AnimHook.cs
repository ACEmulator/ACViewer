using System;
using ACE.DatLoader.Entity;
using ACE.DatLoader.Entity.AnimationHooks;
using ACE.Entity.Enum;
using ACE.Server.Physics.Animation;

namespace ACE.Server.Physics.Hooks
{
    public class AnimHook
    {
        public static void Execute(PhysicsObj obj, AnimationHook animHook)
        {
            switch (animHook.HookType)
            {
                case AnimationHookType.AnimationDone:
                    obj.Hook_AnimDone();
                    break;

                /*case AnimationHookType.Ethereal:
                    if (animHook is EtherealHook hook)
                        obj.set_ethereal(Convert.ToBoolean(hook.Ethereal), false);
                    break;*/

                case AnimationHookType.CreateParticle:
                    if (animHook is CreateParticleHook hook)
                        obj.create_particle_emitter(hook.EmitterInfoId, (int)hook.PartIndex, new AFrame(hook.Offset), (int)hook.EmitterId);
                    break;
            }
        }
    }
}

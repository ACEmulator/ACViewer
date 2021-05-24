using System.Collections.Generic;

using ACE.Entity.Enum;
using ACViewer.Entity.AnimationHooks;

namespace ACViewer.Entity
{
    public class AnimationHook
    {
        public ACE.DatLoader.Entity.AnimationHook _hook;

        public AnimationHook(ACE.DatLoader.Entity.AnimationHook hook)
        {
            _hook = hook;
        }

        public static AnimationHook Create(ACE.DatLoader.Entity.AnimationHook _animationHook)
        {
            switch (_animationHook.HookType)
            {
                case AnimationHookType.AnimationDone:
                    break;

                case AnimationHookType.Attack:
                    return new AttackHook(_animationHook);

                case AnimationHookType.CallPES:
                    return new CallPESHook(_animationHook);

                case AnimationHookType.CreateBlockingParticle:
                    break;

                case AnimationHookType.CreateParticle:
                    return new CreateParticleHook(_animationHook);

                case AnimationHookType.DefaultScript:
                    break;

                case AnimationHookType.DefaultScriptPart:
                    return new DefaultScriptPartHook(_animationHook);

                case AnimationHookType.DestroyParticle:
                    return new DestroyParticleHook(_animationHook);

                case AnimationHookType.Diffuse:
                    return new DiffuseHook(_animationHook);

                case AnimationHookType.DiffusePart:
                    return new DiffusePartHook(_animationHook);

                case AnimationHookType.Ethereal:
                    return new EtherealHook(_animationHook);

                case AnimationHookType.ForceAnimationHook32Bit:
                    break;

                case AnimationHookType.Luminous:
                    return new LuminousHook(_animationHook);

                case AnimationHookType.LuminousPart:
                    return new LuminousPartHook(_animationHook);

                case AnimationHookType.NoDraw:
                    return new NoDrawHook(_animationHook);

                case AnimationHookType.NoOp:
                    break;

                case AnimationHookType.ReplaceObject:
                    return new ReplaceObjectHook(_animationHook);

                case AnimationHookType.Scale:
                    return new ScaleHook(_animationHook);

                case AnimationHookType.SetLight:
                    return new SetLightHook(_animationHook);

                case AnimationHookType.SetOmega:
                    return new SetOmegaHook(_animationHook);

                case AnimationHookType.Sound:
                    return new SoundHook(_animationHook);

                case AnimationHookType.SoundTable:
                    return new SoundTableHook(_animationHook);

                case AnimationHookType.SoundTweaked:
                    return new SoundTweakedHook(_animationHook);

                case AnimationHookType.StopParticle:
                    return new StopParticleHook(_animationHook);

                case AnimationHookType.TextureVelocity:
                    return new TextureVelocityHook(_animationHook);

                case AnimationHookType.TextureVelocityPart:
                    return new TextureVelocityPartHook(_animationHook);

                case AnimationHookType.Transparent:
                    return new TransparentHook(_animationHook);

                case AnimationHookType.TransparentPart:
                    return new TransparentPartHook(_animationHook);
            }
            return new AnimationHook(_animationHook);
        }

        public virtual List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            //treeNode.Add(new TreeNode($"HookType: {_hook.HookType}"));
            treeNode.Add(new TreeNode($"Dir: {_hook.Direction}"));

            return treeNode;
        }
    }
}

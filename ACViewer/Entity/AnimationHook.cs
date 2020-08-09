using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class AnimationHook
    {
        public ACE.DatLoader.Entity.AnimationHook _hook;

        public AnimationHook(ACE.DatLoader.Entity.AnimationHook hook)
        {
            _hook = hook;
        }

        public override string ToString()
        {
            return $"HookType: {_hook.HookType}, Dir: {_hook.Direction}";
        }
    }
}

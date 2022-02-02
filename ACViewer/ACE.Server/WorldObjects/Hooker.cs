using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class Hooker : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Hooker(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Hooker(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            ActivationResponse |= ActivationResponse.Emote;
        }

        public bool IsHooked(WorldObject checker, out Hook hook)
        {
            hook = null;

            if (!OwnerId.HasValue || OwnerId.Value == 0)
                return false;

            //var wo = checker.CurrentLandblock.GetObject(OwnerId.Value);

            //if (wo == null)
                //return false;

            //if (!(wo is Hook _hook))
                //return false;

            //hook = _hook;

            return true;
        }
    }
}

using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class PKModifier : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public PKModifier(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public PKModifier(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        public bool IsPKSwitch => PkLevelModifier == 1;
        public bool IsNPKSwitch => PkLevelModifier == -1;

        private void SetEphemeralValues()
        {
            //CurrentMotionState = new Motion(MotionStance.NonCombat);

            if (IsNPKSwitch)
                ObjectDescriptionFlags |= ObjectDescriptionFlag.NpkSwitch;

            if (IsPKSwitch)
                ObjectDescriptionFlags |= ObjectDescriptionFlag.PkSwitch;
        }
    }
}

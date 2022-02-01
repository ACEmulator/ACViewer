using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class Lifestone : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Lifestone(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Lifestone(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            ObjectDescriptionFlags |= ObjectDescriptionFlag.LifeStone;

            RadarColor = ACE.Entity.Enum.RadarColor.LifeStone;
        }
    }
}

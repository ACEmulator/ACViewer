using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public partial class Corpse : Container
    {
        /// <summary>
        /// The maximum number of seconds for an empty corpse to stick around
        /// </summary>
        public static readonly double EmptyDecayTime = 15.0;

        /// <summary>
        /// Flag indicates if a corpse is from a monster or a player
        /// </summary>
        public bool IsMonster = false;

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Corpse(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Corpse(Biota biota) : base(biota)
        {
            SetEphemeralValues();

            // for player corpses restored from database,
            // ensure any floating corpses fall to the ground
            BumpVelocity = true;
        }

        private void SetEphemeralValues()
        {
            ObjectDescriptionFlags |= ObjectDescriptionFlag.Corpse;

            //CurrentMotionState = new Motion(MotionStance.NonCombat, MotionCommand.Dead);

            ContainerCapacity = 10;
            ItemCapacity = 120;

            SuppressGenerateEffect = true;
        }
    }
}

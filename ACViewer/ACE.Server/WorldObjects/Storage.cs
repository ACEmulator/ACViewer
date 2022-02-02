using ACE.Entity;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class Storage : Chest
    {
        public House House { get => ParentLink as House; }

        public override double Default_ChestResetInterval => double.PositiveInfinity;

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Storage(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Storage(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            IsLocked = false;
            IsOpen = false;

            // unanimated objects will float in the air, and not be affected by gravity
            // unless we give it a bit of velocity to start
            // fixes floating storage chests
            //Velocity = new Vector3(0, 0, 0.5f);
            BumpVelocity = true;
        }
    }
}

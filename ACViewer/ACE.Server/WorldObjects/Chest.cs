using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public partial class Chest : Container
    {
        /// <summary>
        /// This is used for things like Mana Forge Chests
        /// </summary>
        public bool ChestRegenOnClose
        {
            get
            {
                if (ChestResetInterval <= 5)
                    return true;

                return GetProperty(PropertyBool.ChestRegenOnClose) ?? false;
            }
            set { if (!value) RemoveProperty(PropertyBool.ChestRegenOnClose); else SetProperty(PropertyBool.ChestRegenOnClose, value); }
        }

        /// <summary>
        /// This is used for things like Dirty Old Crate
        /// </summary>
        public bool ChestClearedWhenClosed
        {
            get => GetProperty(PropertyBool.ChestClearedWhenClosed) ?? false;
            set { if (!value) RemoveProperty(PropertyBool.ChestClearedWhenClosed); else SetProperty(PropertyBool.ChestClearedWhenClosed, value); }
        }

        /// <summary>
        /// This is the default setup for resetting chests
        /// </summary>
        public double ChestResetInterval
        {
            get
            {
                var chestResetInterval = ResetInterval ?? Default_ChestResetInterval;

                if (chestResetInterval < 15)
                    chestResetInterval = Default_ChestResetInterval;

                return chestResetInterval;
            }
        }

        public virtual double Default_ChestResetInterval => 120;

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Chest(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Chest(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            ContainerCapacity = ContainerCapacity ?? 10;
            ItemCapacity = ItemCapacity ?? 120;

            ActivationResponse |= ActivationResponse.Use;   // todo: fix broken data

            //CurrentMotionState = motionClosed;              // do any chests default to open?

            if (IsLocked)
                DefaultLocked = true;

            //if (DefaultLocked) // ignore regen interval, only regen on relock
                //NextGeneratorRegenerationTime = double.MaxValue;
        }

        public string LockCode
        {
            get => GetProperty(PropertyString.LockCode);
            set { if (value == null) RemoveProperty(PropertyString.LockCode); else SetProperty(PropertyString.LockCode, value); }
        }
    }
}

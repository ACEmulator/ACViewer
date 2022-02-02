using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class Door : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Door(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Door(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            ObjectDescriptionFlags |= ObjectDescriptionFlag.Door;

            if (!DefaultOpen)
            {
                //CurrentMotionState = motionClosed;
                IsOpen = false;
                //Ethereal = false;
            }
            else
            {
                //CurrentMotionState = motionOpen;
                IsOpen = true;
                Ethereal = true;
            }

            ResetInterval = ResetInterval ?? 30.0f;
            LockCode = LockCode ?? "";

            // Account for possible missing property from recreated weenies
            if (IsLocked && !DefaultLocked)
                DefaultLocked = true;

            if (DefaultLocked)
                IsLocked = true;
            else
                IsLocked = false;

            ActivationResponse |= ActivationResponse.Use;
        }

        public string LockCode
        {
            get => GetProperty(PropertyString.LockCode);
            set { if (value == null) RemoveProperty(PropertyString.LockCode); else SetProperty(PropertyString.LockCode, value); }
        }

        public override void SetLinkProperties(WorldObject wo)
        {
            wo.ActivationTarget = Guid.Full;
        }

        public override void OnCollideObject(WorldObject target)
        {
            if (IsOpen) return;

            // currently the only AI options appear to be 0 or 1,
            // 1 meaning able to open doors?
            var creature = target as Creature;
            if (creature == null || creature.AiOptions == 0)
                return;

            //ActOnUse(target);
        }
    }
}

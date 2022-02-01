using ACE.Entity;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class Switch : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Switch(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Switch(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
        }

        public uint? UseTargetAnimation
        {
            get => GetProperty(PropertyDataId.UseTargetAnimation);
            set { if (!value.HasValue) RemoveProperty(PropertyDataId.UseTargetAnimation); else SetProperty(PropertyDataId.UseTargetAnimation, value.Value); }
        }

        public override void SetLinkProperties(WorldObject wo)
        {
            wo.ActivationTarget = Guid.Full;
        }
    }
}

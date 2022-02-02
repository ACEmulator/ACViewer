using ACE.Entity;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class AugmentationDevice : WorldObject
    {
        public long? AugmentationCost
        {
            get => GetProperty(PropertyInt64.AugmentationCost);
            set { if (!value.HasValue) RemoveProperty(PropertyInt64.AugmentationCost); else SetProperty(PropertyInt64.AugmentationCost, value.Value); }
        }

        public int? AugmentationStat
        {
            get => GetProperty(PropertyInt.AugmentationStat);
            set { if (!value.HasValue) RemoveProperty(PropertyInt.AugmentationStat); else SetProperty(PropertyInt.AugmentationStat, value.Value); }
        }

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public AugmentationDevice(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public AugmentationDevice(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
        }
    }
}

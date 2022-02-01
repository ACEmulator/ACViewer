using ACE.Entity;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    /// <summary>
    /// Pet Devices are the essences used to summon creatures
    /// </summary>
    public class PetDevice : WorldObject
    {
        public int? PetClass
        {
            get => GetProperty(PropertyInt.PetClass);
            set { if (value.HasValue) SetProperty(PropertyInt.PetClass, value.Value); else RemoveProperty(PropertyInt.PetClass); }
        }

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public PetDevice(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();

            // todo: remove me when the data is fixed
            Structure = MaxStructure;
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public PetDevice(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
        }
    }
}

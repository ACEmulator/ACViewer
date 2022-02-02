using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class SkillAlterationDevice : WorldObject
    {
        public enum SkillAlterationType
        {
            Undef = 0,
            Specialize = 1,
            Lower = 2,
        }

        public SkillAlterationType TypeOfAlteration
        {
            get => (SkillAlterationType)(GetProperty(PropertyInt.TypeOfAlteration) ?? 0);
            set { if (value == 0) RemoveProperty(PropertyInt.TypeOfAlteration); else SetProperty(PropertyInt.TypeOfAlteration, (int)value); }
        }

        public Skill SkillToBeAltered
        {
            get => (Skill)(GetProperty(PropertyInt.SkillToBeAltered) ?? 0);
            set { if (value == 0) RemoveProperty(PropertyInt.SkillToBeAltered); else SetProperty(PropertyInt.SkillToBeAltered, (int)value); }
        }

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public SkillAlterationDevice(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public SkillAlterationDevice(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
        }
    }
}

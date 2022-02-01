using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;

namespace ACE.Server.WorldObjects
{
    partial class WorldObject
    {
        public Skill WeaponSkill
        {
            get => (Skill)(GetProperty(PropertyInt.WeaponSkill) ?? 0);
            set { if (value == 0) RemoveProperty(PropertyInt.WeaponSkill); else SetProperty(PropertyInt.WeaponSkill, (int)value); }
        }

        public DamageType W_DamageType
        {
            get => (DamageType)(GetProperty(PropertyInt.DamageType) ?? 0);
            set { if (value == 0) RemoveProperty(PropertyInt.DamageType); else SetProperty(PropertyInt.DamageType, (int)value); }
        }

        public AttackType W_AttackType
        {
            get => (AttackType)(GetProperty(PropertyInt.AttackType) ?? 0);
            set { if (value == 0) RemoveProperty(PropertyInt.AttackType); else SetProperty(PropertyInt.AttackType, (int)value); }
        }

        public WeaponType W_WeaponType
        {
            get => (WeaponType)(GetProperty(PropertyInt.WeaponType) ?? 0);
            set { if (value == 0) RemoveProperty(PropertyInt.WeaponType); else SetProperty(PropertyInt.WeaponType, (int)value); }
        }

        public bool AutoWieldLeft
        {
            get => GetProperty(PropertyBool.AutowieldLeft) ?? false;
            set { if (!value) RemoveProperty(PropertyBool.AutowieldLeft); else SetProperty(PropertyBool.AutowieldLeft, value); }
        }

        /// <summary>
        /// Returns TRUE if this weapon cleaves
        /// </summary>
        public bool IsCleaving { get => GetProperty(PropertyInt.Cleaving) != null; }

        /// <summary>
        /// Returns the number of cleave targets for this weapon
        /// If cleaving weapon, this is PropertyInt.Cleaving - 1
        /// </summary>
        public int CleaveTargets
        {
            get
            {
                if (!IsCleaving)
                    return 0;

                return GetProperty(PropertyInt.Cleaving).Value - 1;
            }
        }
    }
}

using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;

namespace ACE.Server.WorldObjects
{
    /// <summary>
    /// Monster combat general functions
    /// </summary>
    partial class Creature
    {
        /// <summary>
        /// Returns true if monster is dead
        /// </summary>
        public bool IsDead => Health.Current <= 0;

        public CombatStyle AiAllowedCombatStyle
        {
            get => (CombatStyle)(GetProperty(PropertyInt.AiAllowedCombatStyle) ?? 0);
            set { if (value == 0) RemoveProperty(PropertyInt.AiAllowedCombatStyle); else SetProperty(PropertyInt.AiAllowedCombatStyle, (int)value); }
        }
    }
}

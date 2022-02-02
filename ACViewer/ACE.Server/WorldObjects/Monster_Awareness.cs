using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;

namespace ACE.Server.WorldObjects
{
    /// <summary>
    /// Determines when a monster wakes up from idle state
    /// </summary>
    partial class Creature
    {
        /// <summary>
        /// Monsters wake up when players are in visual range
        /// </summary>
        public bool IsAwake = false;

        public Tolerance Tolerance
        {
            get => (Tolerance)(GetProperty(PropertyInt.Tolerance) ?? 0);
            set { if (value == 0) RemoveProperty(PropertyInt.Tolerance); else SetProperty(PropertyInt.Tolerance, (int)value); }
        }

        /// <summary>
        /// This list of possible targeting tactics for this monster
        /// </summary>
        public TargetingTactic TargetingTactic
        {
            get => (TargetingTactic)(GetProperty(PropertyInt.TargetingTactic) ?? 0);
            set { if (value == 0) RemoveProperty(PropertyInt.TargetingTactic); else SetProperty(PropertyInt.TargetingTactic, (int)TargetingTactic); }
        }
    }
}

using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class ManaStone : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public ManaStone(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public ManaStone(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
        }

        private double? Efficiency
        {
            get => GetProperty(PropertyFloat.ItemEfficiency);
            set { if (!value.HasValue) RemoveProperty(PropertyFloat.ItemEfficiency); else SetProperty(PropertyFloat.ItemEfficiency, value.Value); }
        }

        private double? DestroyChance
        {
            get => GetProperty(PropertyFloat.ManaStoneDestroyChance);
            set { if (!value.HasValue) RemoveProperty(PropertyFloat.ManaStoneDestroyChance); else SetProperty(PropertyFloat.ManaStoneDestroyChance, value.Value); }
        }

        public void SetUiEffect(Player player, UiEffects effect)
        {
            UiEffects = effect;
            //player.EnqueueBroadcast(new GameMessagePublicUpdatePropertyInt(this, PropertyInt.UiEffects, (int)effect));
        }
    }
}

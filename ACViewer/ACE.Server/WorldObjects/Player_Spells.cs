using System.Collections.Generic;
using System.Linq;

using ACE.Entity.Enum;

namespace ACE.Server.WorldObjects
{
    partial class Player
    {
        public static Dictionary<MagicSchool, uint> FociWCIDs = new Dictionary<MagicSchool, uint>()
        {
            { MagicSchool.CreatureEnchantment, 15268 },   // Foci of Enchantment
            { MagicSchool.ItemEnchantment,     15269 },   // Foci of Artifice
            { MagicSchool.LifeMagic,           15270 },   // Foci of Verdancy
            { MagicSchool.WarMagic,            15271 },   // Foci of Strife
            { MagicSchool.VoidMagic,           43173 },   // Foci of Shadow
        };

        public bool HasFoci(MagicSchool school)
        {
            switch (school)
            {
                case MagicSchool.CreatureEnchantment:
                    if (AugmentationInfusedCreatureMagic > 0)
                        return true;
                    break;
                case MagicSchool.ItemEnchantment:
                    if (AugmentationInfusedItemMagic > 0)
                        return true;
                    break;
                case MagicSchool.LifeMagic:
                    if (AugmentationInfusedLifeMagic > 0)
                        return true;
                    break;
                case MagicSchool.VoidMagic:
                    if (AugmentationInfusedVoidMagic > 0)
                        return true;
                    break;
                case MagicSchool.WarMagic:
                    if (AugmentationInfusedWarMagic > 0)
                        return true;
                    break;
            }

            var wcid = FociWCIDs[school];
            return Inventory.Values.FirstOrDefault(i => i.WeenieClassId == wcid) != null;
        }
    }
}

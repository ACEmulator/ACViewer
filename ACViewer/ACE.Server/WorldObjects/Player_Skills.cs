using System.Collections.Generic;

using ACE.DatLoader;
using ACE.Entity.Enum;

namespace ACE.Server.WorldObjects
{
    partial class Player
    {
        /// <summary>
        /// Returns the XP curve table based on trained or specialized skill
        /// </summary>
        public static List<uint> GetSkillXPTable(SkillAdvancementClass status)
        {
            var xpTable = DatManager.PortalDat.XpTable;

            switch (status)
            {
                case SkillAdvancementClass.Trained:
                    return xpTable.TrainedSkillXpList;

                case SkillAdvancementClass.Specialized:
                    return xpTable.SpecializedSkillXpList;

                default:
                    return null;
            }
        }

        public static HashSet<Skill> MeleeSkills = new HashSet<Skill>()
        {
            Skill.LightWeapons,
            Skill.HeavyWeapons,
            Skill.FinesseWeapons,
            Skill.DualWield,
            Skill.TwoHandedCombat,

            // legacy
            Skill.Axe,
            Skill.Dagger,
            Skill.Mace,
            Skill.Spear,
            Skill.Staff,
            Skill.Sword,
            Skill.UnarmedCombat
        };

        public static HashSet<Skill> MissileSkills = new HashSet<Skill>()
        {
            Skill.MissileWeapons,

            // legacy
            Skill.Bow,
            Skill.Crossbow,
            Skill.Sling,
            Skill.ThrownWeapon
        };

        public static HashSet<Skill> MagicSkills = new HashSet<Skill>()
        {
            Skill.CreatureEnchantment,
            Skill.ItemEnchantment,
            Skill.LifeMagic,
            Skill.VoidMagic,
            Skill.WarMagic
        };
    }
}

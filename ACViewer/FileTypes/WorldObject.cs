using System;
using System.Collections.Generic;
using System.Linq;

using ACE.Common;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.WorldObjects;

using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class WorldObject
    {
        public ACE.Server.WorldObjects.WorldObject _wo;

        public WorldObject(ACE.Server.WorldObjects.WorldObject wo)
        {
            _wo = wo;
        }

        public static readonly Dictionary<PropertyInt, Type> PropertyConverters = new Dictionary<PropertyInt, Type>()
        {
            { PropertyInt.ItemType, typeof(ItemType) },
            { PropertyInt.CreatureType, typeof(CreatureType) },
            { PropertyInt.PaletteTemplate, typeof(PaletteTemplate) },
            { PropertyInt.ClothingPriority, typeof(CoverageMask) },
            { PropertyInt.ValidLocations, typeof(EquipMask) },
            { PropertyInt.CurrentWieldedLocation, typeof(EquipMask) },
            { PropertyInt.ItemUseable, typeof(Usable) },
            { PropertyInt.UiEffects, typeof(UiEffects) },
            { PropertyInt.AccountRequirements, typeof(SubscriptionStatus) },
            { PropertyInt.ArmorType, typeof(ArmorType) },
            { PropertyInt.Bonded, typeof(BondedStatus) },
            { PropertyInt.CombatMode, typeof(CombatMode) },
            { PropertyInt.CurrentAttackHeight, typeof(AttackHeight) },
            { PropertyInt.DamageType, typeof(DamageType) },
            { PropertyInt.DefaultCombatStyle, typeof(CombatStyle) },
            { PropertyInt.AttackType, typeof(AttackType) },
            { PropertyInt.WeaponSkill, typeof(Skill) },
            { PropertyInt.AmmoType, typeof(AmmoType) },
            { PropertyInt.CombatUse, typeof(CombatUse) },
            { PropertyInt.ParentLocation, typeof(ParentLocation) },
            { PropertyInt.Placement, typeof(Placement) },
            { PropertyInt.Tolerance, typeof(Tolerance) },
            { PropertyInt.TargetingTactic, typeof(TargetingTactic) },
            { PropertyInt.HomesickTargetingTactic, typeof(TargetingTactic) },
            { PropertyInt.FriendType, typeof(CreatureType) },
            { PropertyInt.FoeType, typeof(CreatureType) },
            { PropertyInt.MerchandiseItemTypes, typeof(ItemType) },
            { PropertyInt.ActivationResponse, typeof(ActivationResponse) },
            { PropertyInt.BoosterEnum, typeof(PropertyAttribute2nd) },
            { PropertyInt.PhysicsState, typeof(PhysicsState) },
            { PropertyInt.TargetType, typeof(ItemType) },
            { PropertyInt.RadarBlipColor, typeof(RadarColor) },
            { PropertyInt.PkLevelModifier, typeof(PKLevel) },
            { PropertyInt.GeneratorType, typeof(GeneratorType) },
            { PropertyInt.AiAllowedCombatStyle, typeof(CombatStyle) },
            { PropertyInt.GeneratorDestructionType, typeof(GeneratorDestruct) },
            { PropertyInt.PortalBitmask, typeof(PortalBitmask) },
            { PropertyInt.Gender, typeof(Gender) },
            { PropertyInt.Attuned, typeof(AttunedStatus) },
            { PropertyInt.AttackHeight, typeof(AttackHeight) },
            { PropertyInt.CloakStatus, typeof(CloakStatus) },
            { PropertyInt.MaterialType, typeof(MaterialType) },
            { PropertyInt.ShowableOnRadar, typeof(RadarBehavior) },
            { PropertyInt.PlayerKillerStatus, typeof(PlayerKillerStatus) },
            { PropertyInt.AiOptions, typeof(AiOption) },
            { PropertyInt.GeneratorTimeType, typeof(GeneratorTimeType) },
            { PropertyInt.GeneratorEndDestructionType, typeof(GeneratorDestruct) },
            { PropertyInt.HouseStatus, typeof(HouseStatus) },
            { PropertyInt.HookPlacement, typeof(Placement) },
            { PropertyInt.HookType, typeof(HookType) },
            { PropertyInt.HookItemType, typeof(ItemType) },
            { PropertyInt.HouseType, typeof(HouseType) },
            { PropertyInt.WieldRequirements, typeof(WieldRequirement) },
            { PropertyInt.WieldSkillType, typeof(Skill) },  // depends on WieldRequirement, can also be Attribute
            { PropertyInt.SlayerCreatureType, typeof(CreatureType) },
            { PropertyInt.AppraisalLongDescDecoration, typeof(AppraisalLongDescDecorations) },
            { PropertyInt.AppraisalItemSkill, typeof(Skill) },
            { PropertyInt.GemType, typeof(MaterialType) },
            { PropertyInt.ImbuedEffect, typeof(ImbuedEffectType) },
            { PropertyInt.TypeOfAlteration, typeof(SkillAlterationDevice) },
            { PropertyInt.SkillToBeAltered, typeof(Skill) },
            { PropertyInt.HeritageGroup, typeof(HeritageGroup) },
            { PropertyInt.TransferFromAttribute, typeof(PropertyAttribute) },
            { PropertyInt.TransferToAttribute, typeof(PropertyAttribute) },
            { PropertyInt.HookGroup, typeof(HookGroupType) },
            //{ PropertyInt.MeleeDefenseImbuedEffectTypeCache, typeof(ImbuedEffectType) },
            //{ PropertyInt.MissileDefenseImbuedEffectTypeCache, typeof(ImbuedEffectType) },
            //{ PropertyInt.MagicDefenseImbuedEffectTypeCache, typeof(ImbuedEffectType) },
            { PropertyInt.AugmentationStat, typeof(AugmentationType) },
            { PropertyInt.ItemAttributeLimit, typeof(PropertyAttribute) },
            { PropertyInt.ItemAttribute2ndLimit, typeof(PropertyAttribute2nd) },
            { PropertyInt.CharacterTitleId, typeof(CharacterTitle) },
            { PropertyInt.ResistanceModifierType, typeof(DamageType) },
            { PropertyInt.EquipmentSetId, typeof(EquipmentSet) },
            { PropertyInt.WieldRequirements2, typeof(WieldRequirement) },
            { PropertyInt.WieldSkillType2, typeof(Skill) },  // depends on WieldRequirement, can also be Attribute
            { PropertyInt.WieldRequirements3, typeof(WieldRequirement) },
            { PropertyInt.WieldSkillType3, typeof(Skill) },  // depends on WieldRequirement, can also be Attribute
            { PropertyInt.WieldRequirements4, typeof(WieldRequirement) },
            { PropertyInt.WieldSkillType4, typeof(Skill) },  // depends on WieldRequirement, can also be Attribute
            // shared cooldown could map into SpellId
            { PropertyInt.SharedCooldown, typeof(SpellId) },    // handle missing?
            { PropertyInt.Faction1Bits, typeof(FactionBits) },
            { PropertyInt.Faction2Bits, typeof(FactionBits) },
            { PropertyInt.Faction3Bits, typeof(FactionBits) },
            // HatredBits?
            { PropertyInt.ImbuedEffect2, typeof(ImbuedEffectType) },
            { PropertyInt.ImbuedEffect3, typeof(ImbuedEffectType) },
            { PropertyInt.ImbuedEffect4, typeof(ImbuedEffectType) },
            { PropertyInt.ImbuedEffect5, typeof(ImbuedEffectType) },
            // ImbueStackingBits?
            { PropertyInt.AetheriaBitfield, typeof(AetheriaBitfield) },
            { PropertyInt.HeritageSpecificArmor, typeof(HeritageGroup) },
            { PropertyInt.UseCreatesContractId, typeof(ContractId) },
            { PropertyInt.WeaponType, typeof(WeaponType) },
            { PropertyInt.SummoningMastery, typeof(SummoningMastery) },
            { PropertyInt.UseRequiresSkill, typeof(Skill) },
            { PropertyInt.UseRequiresSkillSpec, typeof(Skill) },
            // ace custom
            { PropertyInt.VisualClothingPriority, typeof(CoverageMask) }
        };

        public TreeNode BuildTree()
        {
            var typeStr = _wo.GetType().ToString().Replace("ACE.Server.WorldObjects.", "");

            var treeView = new TreeNode($"{typeStr}: {_wo.WeenieClassId} - {_wo.Name}");

            if (_wo is Creature creature)
            {
                var attributes = new TreeNode($"Attributes");
                attributes.Items.Add(new TreeNode($"Strength: {creature.Strength.Current}"));
                attributes.Items.Add(new TreeNode($"Endurance: {creature.Endurance.Current}"));
                attributes.Items.Add(new TreeNode($"Coordination: {creature.Coordination.Current}"));
                attributes.Items.Add(new TreeNode($"Quickness: {creature.Quickness.Current}"));
                attributes.Items.Add(new TreeNode($"Focus: {creature.Focus.Current}"));
                attributes.Items.Add(new TreeNode($"Self: {creature.Self.Current}"));
                treeView.Items.Add(attributes);

                var vitals = new TreeNode($"Vitals");
                vitals.Items.Add(new TreeNode($"Health: {creature.Health.Current}/{creature.Health.MaxValue}"));
                vitals.Items.Add(new TreeNode($"Stamina: {creature.Stamina.Current}/{creature.Stamina.MaxValue}"));
                vitals.Items.Add(new TreeNode($"Mana: {creature.Mana.Current}/{creature.Mana.MaxValue}"));
                treeView.Items.Add(vitals);

                var _specialized = creature.Skills.Values.Where(s => s.AdvancementClass == SkillAdvancementClass.Specialized).OrderBy(s => s.Skill.ToString()).ToList();
                var _trained = creature.Skills.Values.Where(s => s.AdvancementClass == SkillAdvancementClass.Trained).OrderBy(s => s.Skill.ToString()).ToList();
                var _untrained = creature.Skills.Values.Where(s => s.AdvancementClass == SkillAdvancementClass.Untrained && s.IsUsable).OrderBy(s => s.Skill.ToString()).ToList();
                var _unusable = creature.Skills.Values.Where(s => s.AdvancementClass == SkillAdvancementClass.Untrained && !s.IsUsable).OrderBy(s => s.Skill.ToString()).ToList();

                var skills = new TreeNode($"Skills");

                if (_specialized.Count > 0)
                {
                    var specialized = new TreeNode($"Specialized:");

                    foreach (var skill in _specialized)
                        specialized.Items.Add(new TreeNode($"{skill.Skill.ToSentence()}: {skill.Current}"));

                    skills.Items.Add(specialized);
                }

                if (_trained.Count > 0)
                {
                    var trained = new TreeNode($"Trained:");

                    foreach (var skill in _trained)
                        trained.Items.Add(new TreeNode($"{skill.Skill.ToSentence()}: {skill.Current}"));

                    skills.Items.Add(trained);
                }

                if (_untrained.Count > 0)
                {
                    var untrained = new TreeNode($"Untrained:");

                    foreach (var skill in _untrained)
                        untrained.Items.Add(new TreeNode($"{skill.Skill.ToSentence()}: {skill.Current}"));

                    skills.Items.Add(untrained);
                }

                if (_unusable.Count > 0)
                {
                    var unusable = new TreeNode($"Unusable");

                    foreach (var skill in _unusable)
                        unusable.Items.Add(new TreeNode($"{skill.Skill.ToSentence()}: {skill.Current}"));

                    skills.Items.Add(unusable);
                }

                treeView.Items.Add(skills);
            }

            var propInts = _wo.GetAllPropertyInt();
            var propInt64s = _wo.GetAllPropertyInt64();
            var propBools = _wo.GetAllPropertyBools();
            var propFloats = _wo.GetAllPropertyFloat();
            var propStrings = _wo.GetAllPropertyString();
            var propIIDs = _wo.GetAllPropertyInstanceId();
            var propDIDs = _wo.GetAllPropertyDataId();

            if (propInts.Count > 0)
            {
                var props = new TreeNode($"PropertyInt ({propInts.Count})");

                foreach (var propInt in propInts.OrderBy(i => i.Key))
                {
                    var val = $"{propInt.Value}";

                    if (PropertyConverters.TryGetValue(propInt.Key, out var enumType))
                    {
                        val = $"{System.Enum.ToObject(enumType, propInt.Value)}";
                    }
                    else if (propInt.Key.ToString().EndsWith("Timestamp") || propInt.Key.ToString().EndsWith("Time") || propInt.Key.ToString().EndsWith("Login"))
                    {
                        val = $"{Time.GetDateTimeFromTimestamp(propInt.Value).ToLocalTime()}";
                    }
                    else if (propInt.Key == PropertyInt.TsysMutationData)
                    {
                        val = $"{propInt.Value:X8}";
                    }

                    props.Items.Add(new TreeNode($"{propInt.Key}: {val}"));
                }

                treeView.Items.Add(props);
            }

            if (propInt64s.Count > 0)
            {
                var props = new TreeNode($"PropertyInt64 ({propInt64s.Count})");

                foreach (var propInt64 in propInt64s.OrderBy(i => i.Key))
                    props.Items.Add(new TreeNode($"{propInt64.Key}: {propInt64.Value}"));

                treeView.Items.Add(props);
            }

            if (propBools.Count > 0)
            {
                var props = new TreeNode($"PropertyBool ({propBools.Count})");

                foreach (var propBool in propBools.OrderBy(i => i.Key))
                    props.Items.Add(new TreeNode($"{propBool.Key}: {propBool.Value}"));

                treeView.Items.Add(props);
            }

            if (propFloats.Count > 0)
            {
                var props = new TreeNode($"PropertyFloat ({propFloats.Count})");

                foreach (var propFloat in propFloats.OrderBy(i => i.Key))
                    props.Items.Add(new TreeNode($"{propFloat.Key}: {propFloat.Value}"));

                treeView.Items.Add(props);
            }

            if (propStrings.Count > 0)
            {
                var props = new TreeNode($"PropertyString ({propStrings.Count})");

                foreach (var propString in propStrings.OrderBy(i => i.Key))
                    props.Items.Add(new TreeNode($"{propString.Key}: {propString.Value}"));

                treeView.Items.Add(props);
            }

            if (propIIDs.Count > 0)
            {
                var props = new TreeNode($"PropertyInstanceID ({propIIDs.Count})");

                foreach (var propIID in propIIDs.OrderBy(i => i.Key))
                    props.Items.Add(new TreeNode($"{propIID.Key}: {propIID.Value:X8}"));

                treeView.Items.Add(props);
            }

            if (propDIDs.Count > 0)
            {
                var props = new TreeNode($"PropertyDID ({propDIDs.Count})");

                foreach (var propDID in propDIDs.OrderBy(i => i.Key))
                    props.Items.Add(new TreeNode($"{propDID.Key}: {propDID.Value:X8}", clickable: true));

                treeView.Items.Add(props);
            }

            return treeView;
        }
    }
}

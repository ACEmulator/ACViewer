using System;
using System.Collections.Generic;

using ACE.Common;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.WorldObjects.Entity;

namespace ACE.Server.WorldObjects
{
    public partial class Creature : Container
    {
        public Creature() : base()
        {
            InitializePropertyDictionaries();
            SetEphemeralValues();
        }
        
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Creature(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            InitializePropertyDictionaries();
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Creature(Biota biota) : base(biota)
        {
            InitializePropertyDictionaries();
            SetEphemeralValues();
        }

        private void InitializePropertyDictionaries()
        {
            if (Biota.PropertiesAttribute == null)
                Biota.PropertiesAttribute = new Dictionary<PropertyAttribute, PropertiesAttribute>();
            if (Biota.PropertiesAttribute2nd == null)
                Biota.PropertiesAttribute2nd = new Dictionary<PropertyAttribute2nd, PropertiesAttribute2nd>();
            if (Biota.PropertiesBodyPart == null)
                Biota.PropertiesBodyPart = new Dictionary<CombatBodyPart, PropertiesBodyPart>();
            if (Biota.PropertiesSkill == null)
                Biota.PropertiesSkill = new Dictionary<Skill, PropertiesSkill>();
        }

        private void SetEphemeralValues()
        {
            if (!(this is Player))
                GenerateNewFace();

            // If any of the vitals don't exist for this biota, one will be created automatically in the CreatureVital ctor
            Vitals[PropertyAttribute2nd.MaxHealth] = new CreatureVital(this, PropertyAttribute2nd.MaxHealth);
            Vitals[PropertyAttribute2nd.MaxStamina] = new CreatureVital(this, PropertyAttribute2nd.MaxStamina);
            Vitals[PropertyAttribute2nd.MaxMana] = new CreatureVital(this, PropertyAttribute2nd.MaxMana);

            // If any of the attributes don't exist for this biota, one will be created automatically in the CreatureAttribute ctor
            Attributes[PropertyAttribute.Strength] = new CreatureAttribute(this, PropertyAttribute.Strength);
            Attributes[PropertyAttribute.Endurance] = new CreatureAttribute(this, PropertyAttribute.Endurance);
            Attributes[PropertyAttribute.Coordination] = new CreatureAttribute(this, PropertyAttribute.Coordination);
            Attributes[PropertyAttribute.Quickness] = new CreatureAttribute(this, PropertyAttribute.Quickness);
            Attributes[PropertyAttribute.Focus] = new CreatureAttribute(this, PropertyAttribute.Focus);
            Attributes[PropertyAttribute.Self] = new CreatureAttribute(this, PropertyAttribute.Self);

            foreach (var kvp in Biota.PropertiesSkill)
                Skills[kvp.Key] = new CreatureSkill(this, kvp.Key, kvp.Value);

            if (Health.Current <= 0)
                Health.Current = Health.MaxValue;
            if (Stamina.Current <= 0)
                Stamina.Current = Stamina.MaxValue;
            if (Mana.Current <= 0)
                Mana.Current = Mana.MaxValue;

            if (!(this is Player))
            {
                GenerateWieldList();

                EquipInventoryItems();

                GenerateWieldedTreasure();

                EquipInventoryItems();

                // TODO: fix tod data
                Health.Current = Health.MaxValue;
                Stamina.Current = Stamina.MaxValue;
                Mana.Current = Mana.MaxValue;
            }

            //SetMonsterState();

            //CurrentMotionState = new Motion(MotionStance.NonCombat, MotionCommand.Ready);

            //selectedTargets = new Dictionary<uint, WorldObjectInfo>();
        }

        // verify logic
        public bool IsNPC => !(this is Player) && !Attackable && TargetingTactic == TargetingTactic.None;

        public void GenerateNewFace()
        {
            var cg = DatManager.PortalDat.CharGen;

            if (!Heritage.HasValue)
            {
                if (!string.IsNullOrEmpty(HeritageGroupName) && Enum.TryParse(HeritageGroupName.Replace("'", ""), true, out HeritageGroup heritage))
                    Heritage = (int)heritage;
            }

            if (!Gender.HasValue)
            {
                if (!string.IsNullOrEmpty(Sex) && Enum.TryParse(Sex, true, out Gender gender))
                    Gender = (int)gender;
            }

            if (!Heritage.HasValue || !Gender.HasValue)
            {
#if DEBUG
                //if (!(NpcLooksLikeObject ?? false))
                //log.Debug($"Creature.GenerateNewFace: {Name} (0x{Guid}) - wcid {WeenieClassId} - Heritage: {Heritage} | HeritageGroupName: {HeritageGroupName} | Gender: {Gender} | Sex: {Sex} - Data missing or unparsable, Cannot randomize face.");
#endif
                return;
            }

            if (!cg.HeritageGroups.TryGetValue((uint)Heritage, out var heritageGroup) || !heritageGroup.Genders.TryGetValue((int)Gender, out var sex))
            {
#if DEBUG
                Console.WriteLine($"Creature.GenerateNewFace: {Name} (0x{Guid}) - wcid {WeenieClassId} - Heritage: {Heritage} | HeritageGroupName: {HeritageGroupName} | Gender: {Gender} | Sex: {Sex} - Data invalid, Cannot randomize face.");
#endif
                return;
            }

            PaletteBaseId = sex.BasePalette;

            var appearance = new Appearance
            {
                HairStyle = 1,
                HairColor = 1,
                HairHue = 1,

                EyeColor = 1,
                Eyes = 1,

                Mouth = 1,
                Nose = 1,

                SkinHue = 1
            };

            // Get the hair first, because we need to know if you're bald, and that's the name of that tune!
            if (sex.HairStyleList.Count > 1)
            {
                //if (PropertyManager.GetBool("npc_hairstyle_fullrange").Item)
                    //appearance.HairStyle = (uint)ThreadSafeRandom.Next(0, sex.HairStyleList.Count - 1);
                //else
                    appearance.HairStyle = (uint)ThreadSafeRandom.Next(0, Math.Min(sex.HairStyleList.Count - 1, 8)); // retail range data compiled by OptimShi
            }
            else
                appearance.HairStyle = 0;

            if (sex.HairStyleList.Count < appearance.HairStyle)
            {
                Console.WriteLine($"Creature.GenerateNewFace: {Name} (0x{Guid}) - wcid {WeenieClassId} - HairStyle = {appearance.HairStyle} | HairStyleList.Count = {sex.HairStyleList.Count} - Data invalid, Cannot randomize face.");
                return;
            }

            var hairstyle = sex.HairStyleList[Convert.ToInt32(appearance.HairStyle)];

            appearance.HairColor = (uint)ThreadSafeRandom.Next(0, sex.HairColorList.Count - 1);
            appearance.HairHue = ThreadSafeRandom.Next(0.0f, 1.0f);

            appearance.EyeColor = (uint)ThreadSafeRandom.Next(0, sex.EyeColorList.Count - 1);
            appearance.Eyes = (uint)ThreadSafeRandom.Next(0, sex.EyeStripList.Count - 1);

            appearance.Mouth = (uint)ThreadSafeRandom.Next(0, sex.MouthStripList.Count - 1);

            appearance.Nose = (uint)ThreadSafeRandom.Next(0, sex.NoseStripList.Count - 1);

            appearance.SkinHue = ThreadSafeRandom.Next(0.0f, 1.0f);

            //// Certain races (Undead, Tumeroks, Others?) have multiple body styles available. This is controlled via the "hair style".
            ////if (hairstyle.AlternateSetup > 0)
            ////    character.SetupTableId = hairstyle.AlternateSetup;

            if (!EyesTextureDID.HasValue)
                EyesTextureDID = sex.GetEyeTexture(appearance.Eyes, hairstyle.Bald);
            if (!DefaultEyesTextureDID.HasValue)
                DefaultEyesTextureDID = sex.GetDefaultEyeTexture(appearance.Eyes, hairstyle.Bald);
            if (!NoseTextureDID.HasValue)
                NoseTextureDID = sex.GetNoseTexture(appearance.Nose);
            if (!DefaultNoseTextureDID.HasValue)
                DefaultNoseTextureDID = sex.GetDefaultNoseTexture(appearance.Nose);
            if (!MouthTextureDID.HasValue)
                MouthTextureDID = sex.GetMouthTexture(appearance.Mouth);
            if (!DefaultMouthTextureDID.HasValue)
                DefaultMouthTextureDID = sex.GetDefaultMouthTexture(appearance.Mouth);
            if (!HeadObjectDID.HasValue)
                HeadObjectDID = sex.GetHeadObject(appearance.HairStyle);

            // Skin is stored as PaletteSet (list of Palettes), so we need to read in the set to get the specific palette
            var skinPalSet = DatManager.PortalDat.ReadFromDat<PaletteSet>(sex.SkinPalSet);
            if (!SkinPaletteDID.HasValue)
                SkinPaletteDID = skinPalSet.GetPaletteID(appearance.SkinHue);

            // Hair is stored as PaletteSet (list of Palettes), so we need to read in the set to get the specific palette
            var hairPalSet = DatManager.PortalDat.ReadFromDat<PaletteSet>(sex.HairColorList[Convert.ToInt32(appearance.HairColor)]);
            if (!HairPaletteDID.HasValue)
                HairPaletteDID = hairPalSet.GetPaletteID(appearance.HairHue);

            // Eye Color
            if (!EyesPaletteDID.HasValue)
                EyesPaletteDID = sex.EyeColorList[Convert.ToInt32(appearance.EyeColor)];
        }

        /// <summary>
        /// This will be false when creature is dead and waits for respawn
        /// </summary>
        public bool IsAlive { get => Health.Current > 0; }

        public override void OnCollideObject(WorldObject target)
        {
            /*if (target.ReportCollisions == false)
                return;

            if (target is Door door)
                door.OnCollideObject(this);
            else if (target is Hotspot hotspot)
                hotspot.OnCollideObject(this);*/
        }
    }
}

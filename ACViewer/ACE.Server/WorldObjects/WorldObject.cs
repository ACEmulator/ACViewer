using System.Collections.ObjectModel;
using System.Numerics;

using ACE.Common;
using ACE.Common.Extensions;
using ACE.Database;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.Physics;
using ACE.Server.Physics.Common;

namespace ACE.Server.WorldObjects
{
    public partial class WorldObject
    {
        /// <summary>
        /// If this object was created from a weenie (and not a database biota), this is the source.
        /// You should never manipulate these values. You should only reference these values in extreme cases.
        /// </summary>
        public Weenie Weenie { get; }

        /// <summary>
        /// This is object property overrides that should have come from the shard db (or init to defaults of object is new to this instance).
        /// You should not manipulate these values directly. To manipulate this use the exposed SetProperty and RemoveProperty functions instead.
        /// </summary>
        public Biota Biota { get; }

        /// <summary>
        /// This is just a wrapper around Biota.Id
        /// </summary>
        public ObjectGuid Guid { get; }

        public PhysicsObj PhysicsObj { get; set; }

        public ObjectDescriptionFlag ObjectDescriptionFlags { get; protected set; }

        public bool IsShield { get => CombatUse != null && CombatUse == ACE.Entity.Enum.CombatUse.Shield; }
        // ValidLocations is bugged for some older two-handed weapons, still contains MeleeWeapon instead of TwoHanded?
        //public bool IsTwoHanded { get => CurrentWieldedLocation != null && CurrentWieldedLocation == EquipMask.TwoHanded; }
        public bool IsTwoHanded => WeaponSkill == Skill.TwoHandedCombat;
        public bool IsBow { get => DefaultCombatStyle != null && (DefaultCombatStyle == CombatStyle.Bow || DefaultCombatStyle == CombatStyle.Crossbow); }
        public bool IsAtlatl { get => DefaultCombatStyle != null && DefaultCombatStyle == CombatStyle.Atlatl; }
        public bool IsAmmoLauncher { get => IsBow || IsAtlatl; }
        public bool IsThrownWeapon { get => DefaultCombatStyle != null && DefaultCombatStyle == CombatStyle.ThrownWeapon; }
        public bool IsRanged { get => IsAmmoLauncher || IsThrownWeapon; }
        public bool IsCaster { get => DefaultCombatStyle != null && (DefaultCombatStyle == CombatStyle.Magic); }

        public WorldObject Wielder { get; set; }

        public WorldObject() { }
        
        /// <summary>
        /// A new biota will be created taking all of its values from weenie.
        /// </summary>
        protected WorldObject(Weenie weenie, ObjectGuid guid)
        {
            Weenie = weenie;
            Biota = ACE.Entity.Adapter.WeenieConverter.ConvertToBiota(weenie, guid.Full, false, true);
            Guid = guid;

            InitializePropertyDictionaries();
            SetEphemeralValues();

            CreationTimestamp = (int)Time.GetUnixTime();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// Any properties tagged as Ephemeral will be removed from the biota.
        /// </summary>
        protected WorldObject(Biota biota)
        {
            Biota = biota;
            Guid = new ObjectGuid(Biota.Id);

            //biotaOriginatedFromDatabase = true;

            InitializePropertyDictionaries();
            SetEphemeralValues();
        }

        private void InitializePropertyDictionaries()
        {
            if (Biota.PropertiesEnchantmentRegistry == null)
                Biota.PropertiesEnchantmentRegistry = new Collection<PropertiesEnchantmentRegistry>();
        }

        private void SetEphemeralValues()
        {
            ObjectDescriptionFlags = ObjectDescriptionFlag.Attackable;

            if (Placement == null)
                Placement = ACE.Entity.Enum.Placement.Resting;
        }

        public bool BumpVelocity { get; set; }

        /// <summary>
        /// Initializes a new default physics object
        /// </summary>
        public virtual void InitPhysicsObj()
        {
            //Console.WriteLine($"InitPhysicsObj({Name} - {Guid})");

            var defaultState = CalculatedPhysicsState();

            if (!(this is Creature))
            {
                var isDynamic = Static == null || !Static.Value;
                var setupTableId = SetupTableId;

                // TODO: REMOVE ME?
                // Temporary workaround fix to account for ace spawn placement issues with certain hooked objects.
                if (this is Hook)
                {
                    var hookWeenie = DatabaseManager.World.GetCachedWeenie(WeenieClassId);
                    setupTableId = hookWeenie.GetProperty(PropertyDataId.Setup) ?? SetupTableId;
                }
                // TODO: REMOVE ME?

                PhysicsObj = PhysicsObj.makeObject(setupTableId, Guid.Full, isDynamic);
            }
            else
            {
                PhysicsObj = new PhysicsObj();
                PhysicsObj.makeAnimObject(SetupTableId, true);
            }

            PhysicsObj.set_object_guid(Guid);

            PhysicsObj.set_weenie_obj(new WeenieObject(this));

            PhysicsObj.SetMotionTableID(MotionTableId);

            PhysicsObj.SetScaleStatic(ObjScale ?? 1.0f);

            PhysicsObj.State = defaultState;

            //if (creature != null) AllowEdgeSlide = true;

            if (BumpVelocity)
                PhysicsObj.Velocity = new Vector3(0, 0, 0.5f);
        }

        public virtual bool EnterWorld()
        {
            if (Location == null)
                return false;

            //if (!LandblockManager.AddObject(this))
                //return false;

            //if (SuppressGenerateEffect != true)
                //ApplyVisualEffects(PlayScript.Create);

            //if (Generator != null)
                //OnGeneration(Generator);

            //Console.WriteLine($"{Name}.EnterWorld()");

            return true;
        }
        
        /// <summary>
        /// Returns TRUE if this WorldObject is a generic linkspot
        /// Linkspots are used for things like Houses,
        /// where the portal destination should be populated at runtime.
        /// </summary>
        public bool IsLinkSpot => WeenieType == WeenieType.Generic && WeenieClassName.Equals("portaldestination");      // TODO: change to wcid

        public DestinationType DestinationType { get; set; }

        public virtual void OnCollideObject(WorldObject target)
        {
        }

        public virtual void OnCollideObjectEnd(WorldObject target)
        {
            // empty base
        }

        public virtual void OnCollideEnvironment()
        {
        }

        public void SetStance(MotionStance stance, bool broadcast = true)
        {
            /*var motion = new Motion(stance);

            CurrentMotionState = motion;

            if (broadcast)
                EnqueueBroadcastMotion(CurrentMotionState);*/
        }

        public string GetPluralName()
        {
            var pluralName = PluralName;

            if (pluralName == null)
                pluralName = Name.Pluralize();

            return pluralName;
        }
    }
}

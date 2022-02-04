using System;
using System.Collections.ObjectModel;
using System.Numerics;

using ACE.Common;
using ACE.Common.Extensions;
using ACE.Database;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.Managers;
using ACE.Server.Physics;
using ACE.Server.Physics.Common;
using ACE.Server.Physics.Util;

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

        public WorldObject()
        {
            Biota = new Biota();
        }
        
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
            InitializeGenerator();

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
            InitializeGenerator();
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

        public bool AddPhysicsObj(Physics.Common.Position location)
        {
            if (PhysicsObj.CurCell != null)
                return false;

            AdjustDungeonCells(location);

            // exclude linkspots from spawning
            if (WeenieClassId == 10762) return true;
            
            var success = PhysicsObj.enter_world(location);

            if (!success || PhysicsObj.CurCell == null)
            {
                PhysicsObj.DestroyObject();
                PhysicsObj = null;
                return false;
            }

            Location = PhysicsObj.Position.ToACE();

            return true;
        }

        // todo: This should really be an extension method for Position, or a static method within Position or even AdjustPos
        public static bool AdjustDungeonCells(Physics.Common.Position pos)
        {
            if (pos == null) return false;

            var landblock = LScape.get_landblock(pos.ObjCellID);
            if (landblock == null || !landblock.HasDungeon) return false;

            var dungeonID = pos.ObjCellID >> 16;

            var adjustCell = AdjustCell.Get(dungeonID);
            var cellID = adjustCell.GetCell(pos.Frame.Origin);

            if (cellID != null && pos.ObjCellID != cellID.Value)
            {
                pos.ObjCellID = cellID.Value;
                return true;
            }
            return false;
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
            return ACViewer.Server.EnterWorld(this);
        }
        
        /// <summary>
        /// Returns TRUE if this WorldObject is a generic linkspot
        /// Linkspots are used for things like Houses,
        /// where the portal destination should be populated at runtime.
        /// </summary>
        public bool IsLinkSpot => WeenieType == WeenieType.Generic && WeenieClassName.Equals("portaldestination");      // TODO: change to wcid

        public SetPosition ScatterPos { get; set; }

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

        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// If this is a container or a creature, all of the inventory and/or equipped objects will also be destroyed.<para />
        /// An object should only be destroyed once.
        /// </summary>
        public void Destroy(bool raiseNotifyOfDestructionEvent = true, bool fromLandblockUnload = false)
        {
            if (IsDestroyed)
            {
                Console.WriteLine("Item 0x{0:X8}:{1} called destroy more than once.", Guid.Full, Name);
                return;
            }

            IsDestroyed = true;

            ReleasedTimestamp = Time.GetUnixTime();

            if (this is Container container)
            {
                foreach (var item in container.Inventory.Values)
                    item.Destroy();
            }

            if (this is Creature creature)
            {
                foreach (var item in creature.EquippedObjects.Values)
                    item.Destroy();
            }

            //if (this is Pet pet && pet.P_PetOwner?.CurrentActivePet == this)
                //pet.P_PetOwner.CurrentActivePet = null;

            /*if (this is Vendor vendor)
            {
                foreach (var wo in vendor.DefaultItemsForSale.Values)
                    wo.Destroy();

                foreach (var wo in vendor.UniqueItemsForSale.Values)
                    wo.Destroy();
            }*/

            if (raiseNotifyOfDestructionEvent)
                NotifyOfEvent(RegenerationType.Destruction);

            if (IsGenerator)
            {
                if (fromLandblockUnload)
                    ProcessGeneratorDestructionDirective(GeneratorDestruct.Destroy, fromLandblockUnload);
                else
                    OnGeneratorDestroy();
            }

            //CurrentLandblock?.RemoveWorldObject(Guid);
            if (PhysicsObj != null)
                PhysicsObj.DestroyObject();
 
            //RemoveBiotaFromDatabase();

            if (Guid.IsDynamic())
                GuidManager.RecycleDynamicGuid(Guid);
        }
    }
}

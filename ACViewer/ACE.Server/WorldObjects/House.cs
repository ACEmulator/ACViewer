using System;
using System.Collections.Generic;
using System.Linq;

using ACE.Database;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.Entity;

namespace ACE.Server.WorldObjects
{
    public class House : WorldObject
    {
        /// <summary>
        /// house open/closed status
        /// 0 = closed, 1 = open
        /// </summary>
        public bool OpenStatus { get => OpenToEveryone; set => OpenToEveryone = value; }
        
        /// <summary>
        /// For linking mansions
        /// </summary>
        public List<House> LinkedHouses;

        public SlumLord SlumLord { get => ChildLinks.FirstOrDefault(l => l as SlumLord != null) as SlumLord; }
        public List<Hook> Hooks { get => ChildLinks.OfType<Hook>().ToList(); }
        public List<Storage> Storage { get => ChildLinks.OfType<Storage>().ToList(); }
        public WorldObject BootSpot => ChildLinks.FirstOrDefault(i => i.WeenieType == WeenieType.BootSpot);

        public HousePortal HousePortal { get => ChildLinks.FirstOrDefault(l => l as HousePortal != null) as HousePortal; }
        public List<WorldObject> Linkspots => ChildLinks.Where(l => l.WeenieType == WeenieType.Generic && l.WeenieClassName.Equals("portaldestination")).ToList();

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public House(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            InitializePropertyDictionaries();
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public House(Biota biota) : base(biota)
        {
            InitializePropertyDictionaries();
            SetEphemeralValues();
        }

        private void InitializePropertyDictionaries()
        {
            if (Biota.HousePermissions == null)
                Biota.HousePermissions = new Dictionary<uint, bool>();
        }

        private void SetEphemeralValues()
        {
            DefaultScriptId = (uint)PlayScript.RestrictionEffectBlue;

            //BuildGuests();

            LinkedHouses = new List<House>();
        }

        public override void SetLinkProperties(WorldObject wo)
        {
            // for house dungeons, link to outdoor house properties
            var house = this;
            /*if (CurrentLandblock != null && CurrentLandblock.HasDungeon && HouseType != HouseType.Apartment)
            {
                var biota = DatabaseManager.Shard.BaseDatabase.GetBiotasByWcid(WeenieClassId).Where(bio => bio.BiotaPropertiesPosition.Count > 0).FirstOrDefault(b => b.BiotaPropertiesPosition.FirstOrDefault(p => p.PositionType == (ushort)PositionType.Location).ObjCellId >> 16 != Location.Landblock);
                if (biota != null)
                {
                    house = WorldObjectFactory.CreateWorldObject(biota) as House;
                    HouseOwner = house.HouseOwner;
                    HouseOwnerName = house.HouseOwnerName;
                }
            }*/

            //Console.WriteLine($"House.SetLinkProperties({wo.Name}) (0x{wo.Guid}): WeenieType {wo.WeenieType} | HouseId:{house.HouseId} | HouseOwner: {house.HouseOwner} | HouseOwnerName: {house.HouseOwnerName}");

            wo.HouseId = house.HouseId;
            wo.HouseOwner = house.HouseOwner;
            //wo.HouseInstance = house.HouseInstance;
            //wo.HouseOwnerName = house.HouseOwnerName;

            //if (house.HouseOwner != null && wo is SlumLord)
                //wo.CurrentMotionState = new Motion(MotionStance.Invalid, MotionCommand.On);

            // the inventory items haven't been loaded yet
            if (wo is Hook hook)
            {
                if (hook.HasItem)
                {
                    hook.NoDraw = false;
                    hook.UiHidden = false;
                    hook.Ethereal = false;
                }
                else if (!(house.HouseHooksVisible ?? true))
                {
                    hook.NoDraw = true;
                    hook.UiHidden = true;
                    hook.Ethereal = true;
                }
            }

            //if (wo.IsLinkSpot)
            //{
            //    var housePortals = GetHousePortals();
            //    if (housePortals.Count == 0)
            //    {
            //        Console.WriteLine($"{Name}.SetLinkProperties({wo.Name}): found LinkSpot, but empty HousePortals");
            //        return;
            //    }
            //    var i = housePortals[0];
            //    var destination = new Position(i.ObjCellId, new Vector3(i.OriginX, i.OriginY, i.OriginZ), new Quaternion(i.AnglesX, i.AnglesY, i.AnglesZ, i.AnglesW));

            //    wo.SetPosition(PositionType.Destination, destination);
            //}

            //if (HouseOwner != null)
            //Console.WriteLine($"{Name}.SetLinkProperties({wo.Name}) - houseID: {HouseId:X8}, owner: {HouseOwner:X8}, instance: {HouseInstance:X8}");
        }

        public override void UpdateLinkProperties(WorldObject wo)
        {
            if (wo.HouseOwner != HouseOwner)
            {
                //Console.WriteLine($"{Name}.UpdateLinkProperties({wo.Name} - {wo.Guid}) - HouseOwner: {HouseOwner:X8}");
                //wo.EnqueueBroadcast(new GameMessagePublicUpdateInstanceID(wo, PropertyInstanceId.HouseOwner, new ObjectGuid(HouseOwner ?? 0)));
            }

            SetLinkProperties(wo);
        }

        public bool IsApartment => HouseType == HouseType.Apartment;

        public bool? HouseHooksVisible
        {
            get => GetProperty(PropertyBool.HouseHooksVisible);
            set { if (!value.HasValue) RemoveProperty(PropertyBool.HouseHooksVisible); else SetProperty(PropertyBool.HouseHooksVisible, value.Value); }
        }

        /// <summary>
        /// Returns the database HousePortals for a HouseID
        /// </summary>
        public List<Database.Models.World.HousePortal> GetHousePortals()
        {
            // the database house portals are different from the HousePortal weenie objects
            // the db info contains the portal destinations

            return DatabaseManager.World.GetCachedHousePortals(HouseId.Value);
        }

        public bool HasDungeon => HousePortal != null;

        private uint? _dungeonLandblockID;

        public uint DungeonLandblockID
        {
            get
            {
                if (_dungeonLandblockID == null)
                {
                    var rootHouseBlock = RootHouse.Location.LandblockId.Raw | 0xFFFF;

                    var housePortals = GetHousePortals();

                    var dungeonPortal = housePortals.FirstOrDefault(i => (i.ObjCellId | 0xFFFF) != rootHouseBlock);

                    if (dungeonPortal == null)
                        return 0;

                    _dungeonLandblockID = dungeonPortal.ObjCellId | 0xFFFF;
                }
                return _dungeonLandblockID.Value;
            }
        }

        private uint? _dungeonHouseGuid;

        public uint DungeonHouseGuid
        {
            get
            {
                if (_dungeonHouseGuid == null)
                {
                    if (DungeonLandblockID == 0)
                        return 0;

                    var landblock = (ushort)((DungeonLandblockID >> 16) & 0xFFFF);

                    var basementGuid = DatabaseManager.World.GetCachedBasementHouseGuid(landblock);

                    if (basementGuid == 0)
                        return 0;

                    _dungeonHouseGuid = basementGuid;

                }
                return _dungeonHouseGuid.Value;
            }
        }

        //private House _rootHouse;

        /// <summary>
        /// For villas and mansions, the basement dungeons contain their own House weenie
        /// This dungeon House needs to reference the main outdoor house for various operations,
        /// such as returning the permissions list.
        /// </summary>
        public House RootHouse
        {
            get
            {
                //if (HouseType == ACE.Entity.Enum.HouseType.Apartment || HouseType == ACE.Entity.Enum.HouseType.Cottage)
                //return this;

                var landblock = (ushort)((RootGuid.Full >> 12) & 0xFFFF);

                var landblockId = new LandblockId((uint)(landblock << 16 | 0xFFFF));
                //var isLoaded = LandblockManager.IsLoaded(landblockId);

                /*if (!isLoaded)
                {
                    //if (_rootHouse == null)
                    //_rootHouse = Load(RootGuid.Full);

                    //return _rootHouse;  // return offline copy
                    // do not cache, in case permissions have changed
                    return Load(RootGuid.Full);
                }*/

                /*var loaded = LandblockManager.GetLandblock(landblockId, false);
                return loaded.GetObject(RootGuid) as House;*/
                return null;
            }
        }

        private ObjectGuid? _rootGuid;

        public ObjectGuid RootGuid
        {
            get
            {
                if (_rootGuid == null)
                {
                    if (HouseCell.RootGuids.TryGetValue(Guid.Full, out var rootGuid))
                        _rootGuid = new ObjectGuid(rootGuid);
                    else
                    {
                        Console.WriteLine($"House.RootGuid - couldn't find root guid for house guid {Guid}");
                        _rootGuid = Guid;
                    }
                }
                return _rootGuid.Value;
            }
        }

        public bool OpenToEveryone
        {
            get => (GetProperty(PropertyInt.OpenToEveryone) ?? 0) == 1;
            set { if (!value) RemoveProperty(PropertyInt.OpenToEveryone); else SetProperty(PropertyInt.OpenToEveryone, 1); }
        }

        public int HouseMaxHooksUsable
        {
            get => GetProperty(PropertyInt.HouseMaxHooksUsable) ?? 25;
            set { if (value == 25) RemoveProperty(PropertyInt.HouseMaxHooksUsable); else SetProperty(PropertyInt.HouseMaxHooksUsable, value); }
        }

        public int HouseCurrentHooksUsable
        {
            get => GetProperty(PropertyInt.HouseCurrentHooksUsable) ?? HouseMaxHooksUsable;
            set { if (value == HouseMaxHooksUsable) RemoveProperty(PropertyInt.HouseCurrentHooksUsable); else SetProperty(PropertyInt.HouseCurrentHooksUsable, value); }
        }

        public static Dictionary<HouseType, Dictionary<HookGroupType, int>> HookGroupLimits = new Dictionary<HouseType, Dictionary<HookGroupType, int>>()
        {
            { HouseType.Undef, new Dictionary<HookGroupType, int> {
                { HookGroupType.Undef,                          -1 },
                { HookGroupType.NoisemakingItems,               -1 },
                { HookGroupType.TestItems,                      -1 },
                { HookGroupType.PortalItems,                    -1 },
                { HookGroupType.WritableItems,                  -1 },
                { HookGroupType.SpellCastingItems,              -1 },
                { HookGroupType.SpellTeachingItems,             -1 } }
            },
            { HouseType.Cottage, new Dictionary<HookGroupType, int> {
                { HookGroupType.Undef,                          -1 },
                { HookGroupType.NoisemakingItems,               -1 },
                { HookGroupType.TestItems,                      -1 },
                { HookGroupType.PortalItems,                    -1 },
                { HookGroupType.WritableItems,                   1 },
                { HookGroupType.SpellCastingItems,               5 },
                { HookGroupType.SpellTeachingItems,              0 } }
            },
            { HouseType.Villa, new Dictionary<HookGroupType, int> {
                { HookGroupType.Undef,                          -1 },
                { HookGroupType.NoisemakingItems,               -1 },
                { HookGroupType.TestItems,                      -1 },
                { HookGroupType.PortalItems,                    -1 },
                { HookGroupType.WritableItems,                   1 },
                { HookGroupType.SpellCastingItems,              10 },
                { HookGroupType.SpellTeachingItems,              0 } }
            },
            { HouseType.Mansion, new Dictionary<HookGroupType, int> {
                { HookGroupType.Undef,                          -1 },
                { HookGroupType.NoisemakingItems,               -1 },
                { HookGroupType.TestItems,                      -1 },
                { HookGroupType.PortalItems,                    -1 },
                { HookGroupType.WritableItems,                   3 },
                { HookGroupType.SpellCastingItems,              15 },
                { HookGroupType.SpellTeachingItems,              1 } }
            },
            { HouseType.Apartment, new Dictionary<HookGroupType, int> {
                { HookGroupType.Undef,                          -1 },
                { HookGroupType.NoisemakingItems,               -1 },
                { HookGroupType.TestItems,                      -1 },
                { HookGroupType.PortalItems,                     0 },
                { HookGroupType.WritableItems,                   0 },
                { HookGroupType.SpellCastingItems,              -1 },
                { HookGroupType.SpellTeachingItems,              0 } }
            }
        };

        public int GetHookGroupCurrentCount(HookGroupType hookGroupType) => Hooks.Count(h => h.HasItem && (h.Item?.HookGroup ?? HookGroupType.Undef) == hookGroupType);

        public int GetHookGroupMaxCount(HookGroupType hookGroupType) => HookGroupLimits[HouseType][hookGroupType];
    }
}

using System.Collections.Generic;
using System.Linq;

using ACE.Common;
using ACE.Database;
using ACE.Database.Models.World;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.Entity;
using ACE.Server.Factories;

namespace ACE.Server.WorldObjects
{
    partial class Creature
    {
        public bool EquippedObjectsLoaded { get; private set; }

        /// <summary>
        /// Use EquipObject() and DequipObject() to manipulate this dictionary..<para />
        /// Do not manipulate this dictionary directly.
        /// </summary>
        public Dictionary<ObjectGuid, WorldObject> EquippedObjects { get; } = new Dictionary<ObjectGuid, WorldObject>();

        /// <summary>
        /// The only time this should be used is to populate EquippedObjects from the ctor.
        /// </summary>
        protected void AddBiotasToEquippedObjects(IEnumerable<ACE.Database.Models.Shard.Biota> wieldedItems)
        {
            foreach (var biota in wieldedItems)
            {
                var worldObject = WorldObjectFactory.CreateWorldObject(biota);
                EquippedObjects[worldObject.Guid] = worldObject;

                //AddItemToEquippedItemsRatingCache(worldObject);

                EncumbranceVal += (worldObject.EncumbranceVal ?? 0);
            }

            EquippedObjectsLoaded = true;

            SetChildren();
        }

        /// <summary>
        /// This is called prior to SendSelf to load up the child list for wielded items that are held in a hand.
        /// </summary>
        private void SetChildren()
        {
            Children.Clear();

            foreach (var item in EquippedObjects.Values)
            {
                if (item.CurrentWieldedLocation != null)
                    TrySetChild(item);
            }
        }

        private static void GetPlacementLocation(WorldObject item, EquipMask wieldedLocation, out Placement placement, out ParentLocation parentLocation)
        {
            switch (wieldedLocation)
            {
                case EquipMask.MeleeWeapon:
                case EquipMask.Held:
                case EquipMask.TwoHanded:
                    placement = ACE.Entity.Enum.Placement.RightHandCombat;
                    parentLocation = ACE.Entity.Enum.ParentLocation.RightHand;
                    break;

                case EquipMask.Shield:
                    if (item.ItemType == ItemType.Armor)
                    {
                        placement = ACE.Entity.Enum.Placement.Shield;
                        parentLocation = ACE.Entity.Enum.ParentLocation.Shield;
                    }
                    else
                    {
                        placement = ACE.Entity.Enum.Placement.RightHandNonCombat;
                        parentLocation = ACE.Entity.Enum.ParentLocation.LeftWeapon;
                    }
                    break;

                case EquipMask.MissileWeapon:
                    if (item.DefaultCombatStyle == CombatStyle.Bow || item.DefaultCombatStyle == CombatStyle.Crossbow)
                    {
                        placement = ACE.Entity.Enum.Placement.LeftHand;
                        parentLocation = ACE.Entity.Enum.ParentLocation.LeftHand;
                    }
                    else
                    {
                        placement = ACE.Entity.Enum.Placement.RightHandCombat;
                        parentLocation = ACE.Entity.Enum.ParentLocation.RightHand;
                    }
                    break;

                default:
                    placement = ACE.Entity.Enum.Placement.Default;
                    parentLocation = ACE.Entity.Enum.ParentLocation.None;
                    break;
            }
        }

        /// <summary>
        /// This method sets properties needed for items that will be child items.<para />
        /// Items here are only items equipped in the hands.<para />
        /// This deals with the orientation and positioning for visual appearance of the child items held by the parent.<para />
        /// If the item isn't in a valid child state (CurrentWieldedLocation), the child properties will be cleared. (Placement, ParentLocation, Location).
        /// </summary>
        private bool TrySetChild(WorldObject item)
        {
            if (!IsInChildLocation(item))
            {
                ClearChild(item);
                return false;
            }

            GetPlacementLocation(item, item.CurrentWieldedLocation ?? 0, out var placement, out var parentLocation);

            Children.Add(new HeldItem(item.Guid.Full, (int)parentLocation, (EquipMask)item.CurrentWieldedLocation));

            item.Placement = placement;
            item.ParentLocation = parentLocation;
            item.Location = Location;

            return true;
        }

        protected bool IsInChildLocation(WorldObject item)
        {
            if (item.CurrentWieldedLocation == null)
                return false;

            if (((EquipMask)item.CurrentWieldedLocation & EquipMask.Selectable) != 0)
                return true;

            if (((EquipMask)item.CurrentWieldedLocation & EquipMask.MissileAmmo) != 0)
            {
                var wielder = item.Wielder;

                if (wielder != null && wielder is Creature creature)
                {
                    var weapon = creature.GetEquippedMissileWeapon();

                    if (weapon == null)
                        return false;

                    if (creature.CombatMode == CombatMode.Missile && weapon.WeenieType == WeenieType.MissileLauncher)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This clears the child properties:<para />
        /// Placement = ACE.Entity.Enum.Placement.Resting<para />
        /// ParentLocation = null<para />
        /// Location = null
        /// </summary>
        protected void ClearChild(WorldObject item)
        {
            item.Placement = ACE.Entity.Enum.Placement.Resting;
            item.ParentLocation = null;
            item.Location = null;
        }

        /// <summary>
        /// Returns the currently equipped missile weapon
        /// This can be either a missile launcher (bow, crossbow, atlatl) or stackable thrown weapons directly in the main hand slot
        /// </summary>
        public WorldObject GetEquippedMissileWeapon()
        {
            return EquippedObjects.Values.FirstOrDefault(e => e.CurrentWieldedLocation == EquipMask.MissileWeapon);
        }

        public void GenerateWieldList()
        {
            if (Biota.PropertiesCreateList == null)
                return;

            var wielded = Biota.PropertiesCreateList.Where(i => (i.DestinationType & DestinationType.Wield) != 0).ToList();

            var items = CreateListSelect(wielded);

            foreach (var item in items)
            {
                var wo = WorldObjectFactory.CreateNewWorldObject(item);

                if (wo == null) continue;

                if (wo.ValidLocations == null || (ItemCapacity ?? 0) > 0)
                {
                    if (!TryAddToInventory(wo))
                    {
                        //wo.Destroy();
                    }
                }
                //else
                //TryWieldObject(wo, (EquipMask)wo.ValidLocations);
            }
        }

        public static List<PropertiesCreateList> CreateListSelect(List<PropertiesCreateList> createList)
        {
            /*var trophy_drop_rate = PropertyManager.GetDouble("trophy_drop_rate").Item;
            if (trophy_drop_rate != 1.0)
                return CreateListSelect(createList, (float)trophy_drop_rate);*/

            var rng = ThreadSafeRandom.Next(0.0f, 1.0f);
            var totalProbability = 0.0f;
            var rngSelected = false;

            var results = new List<PropertiesCreateList>();

            foreach (var item in createList)
            {
                var destinationType = item.DestinationType;
                var useRNG = destinationType.HasFlag(DestinationType.Treasure) && item.Shade != 0;

                var shadeOrProbability = item.Shade;

                if (useRNG)
                {
                    // handle sets in 0-1 chunks
                    if (totalProbability >= 1.0f)
                    {
                        totalProbability = 0.0f;
                        rng = ThreadSafeRandom.Next(0.0f, 1.0f);
                        rngSelected = false;
                    }

                    var probability = shadeOrProbability;

                    totalProbability += probability;

                    if (rngSelected || rng >= totalProbability)
                        continue;

                    rngSelected = true;
                }

                results.Add(item);
            }

            return results;
        }

        protected bool TryWieldObjectWithBroadcasting(WorldObject worldObject, EquipMask wieldedLocation)
        {
            // check wield requirements?
            if (!TryEquipObjectWithBroadcasting(worldObject, wieldedLocation))
                return false;

            //TryActivateItemSpells(worldObject);

            return true;
        }

        /// <summary>
        /// This will set the CurrentWieldedLocation property to wieldedLocation and the Wielder property to this guid and will add it to the EquippedObjects dictionary.<para />
        /// It will also increase the EncumbranceVal and Value.
        /// </summary>
        protected bool TryEquipObjectWithBroadcasting(WorldObject worldObject, EquipMask wieldedLocation)
        {
            if (!TryEquipObject(worldObject, wieldedLocation))
                return false;

            /*if (IsInChildLocation(worldObject)) // Is this equipped item visible to others?
                EnqueueBroadcast(false, new GameMessageSound(Guid, Sound.WieldObject));

            if (worldObject.ParentLocation != null)
                EnqueueBroadcast(new GameMessageParentEvent(this, worldObject));

            EnqueueBroadcast(new GameMessageObjDescEvent(this));

            // Notify viewers in the area that we've equipped the item
            EnqueueActionBroadcast(p => p.TrackEquippedObject(this, worldObject));*/

            return true;
        }

        /// <summary>
        /// This will set the CurrentWieldedLocation property to wieldedLocation and the Wielder property to this guid and will add it to the EquippedObjects dictionary.<para />
        /// It will also increase the EncumbranceVal and Value.
        /// </summary>
        public bool TryEquipObject(WorldObject worldObject, EquipMask wieldedLocation)
        {
            // todo: verify wielded location is valid location
            if (!WieldedLocationIsAvailable(worldObject, wieldedLocation))
                return false;

            worldObject.CurrentWieldedLocation = wieldedLocation;
            worldObject.WielderId = Biota.Id;
            worldObject.Wielder = this;

            EquippedObjects[worldObject.Guid] = worldObject;

            //AddItemToEquippedItemsRatingCache(worldObject);

            EncumbranceVal += (worldObject.EncumbranceVal ?? 0);
            Value += (worldObject.Value ?? 0);

            TrySetChild(worldObject);

            //worldObject.OnWield(this);

            return true;
        }

        public bool WieldedLocationIsAvailable(WorldObject item, EquipMask wieldedLocation)
        {
            // filtering to just armor here, or else trinkets and dual wielding breaks
            // update: cannot repro the break anymore?
            //var existing = this is Player ? GetEquippedClothingArmor(item.ClothingPriority ?? 0) : GetEquippedItems(item, wieldedLocation);
            var existing = GetEquippedItems(item, wieldedLocation);

            // TODO: handle overlap from MeleeWeapon / MissileWeapon / Held

            return existing.Count == 0;
        }

        /// <summary>
        /// Returns a list of equipped items with any overlap with input locations
        /// </summary>
        public List<WorldObject> GetEquippedItems(WorldObject item, EquipMask wieldedLocation)
        {
            if (IsWeaponSlot(wieldedLocation))
            {
                // TODO: change to coalesced CurrentWieldedLocation
                GetPlacementLocation(item, wieldedLocation, out var placement, out var parentLocation);
                return EquippedObjects.Values.Where(i => i.ParentLocation != null && i.ParentLocation == parentLocation && i.CurrentWieldedLocation != EquipMask.MissileAmmo).ToList();
            }

            if (item is Clothing)
                return GetEquippedClothingArmor(item.ClothingPriority ?? 0);
            else
                return EquippedObjects.Values.Where(i => i.CurrentWieldedLocation != null && (i.CurrentWieldedLocation & wieldedLocation) != 0).ToList();
        }

        private static bool IsWeaponSlot(EquipMask equipMask)
        {
            switch (equipMask)
            {
                case EquipMask.MeleeWeapon:
                case EquipMask.Held:
                case EquipMask.TwoHanded:
                case EquipMask.Shield:
                case EquipMask.MissileWeapon:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns a list of equipped clothing/armor with any coverage overlap
        /// </summary>
        public List<WorldObject> GetEquippedClothingArmor(CoverageMask coverageMask)
        {
            return EquippedObjects.Values.Where(i => i.ClothingPriority != null && (i.ClothingPriority & coverageMask) != 0).ToList();
        }

        public uint? WieldedTreasureType
        {
            get => GetProperty(PropertyDataId.WieldedTreasureType);
            set { if (!value.HasValue) RemoveProperty(PropertyDataId.WieldedTreasureType); else SetProperty(PropertyDataId.WieldedTreasureType, value.Value); }
        }

        public List<TreasureWielded> WieldedTreasure
        {
            get
            {
                if (WieldedTreasureType.HasValue)
                    return DatabaseManager.World.GetCachedWieldedTreasure(WieldedTreasureType.Value);

                return null;
            }
        }

        /// <summary>
        /// Try to wield an object for non-player creatures
        /// </summary>
        /// <returns></returns>
        public bool TryWieldObject(WorldObject worldObject, EquipMask wieldedLocation)
        {
            // check wield requirements?
            if (!TryEquipObject(worldObject, wieldedLocation))
                return false;

            // enqueue to ensure parent object has spawned,
            // and spell fx are visible
            /*var actionChain = new ActionChain();
            actionChain.AddDelaySeconds(0.1);
            actionChain.AddAction(this, () => TryActivateItemSpells(worldObject));
            actionChain.EnqueueChain();*/

            return true;
        }

        public void GenerateWieldedTreasure()
        {
            if (WieldedTreasure == null) return;

            //var table = new TreasureWieldedTable(WieldedTreasure);

            var wieldedTreasure = GenerateWieldedTreasureSets(WieldedTreasure);

            if (wieldedTreasure == null)
                return;

            foreach (var item in wieldedTreasure)
            {
                //if (item.ValidLocations == null || (ItemCapacity ?? 0) > 0)
                {
                    if (!TryAddToInventory(item))
                        item.Destroy();
                }
                //else
                //TryWieldObject(item, (EquipMask)item.ValidLocations);
            }
        }
    }
}

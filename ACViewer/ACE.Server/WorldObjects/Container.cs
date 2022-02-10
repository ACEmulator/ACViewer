using System;
using System.Collections.Generic;
using System.Linq;

using ACE.Database;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.Factories;

namespace ACE.Server.WorldObjects
{
    public partial class Container : WorldObject
    {
        public Container() : base()
        {

        }
        
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Container(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            InitializePropertyDictionaries();
            SetEphemeralValues(false);

            InventoryLoaded = true;
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Container(Biota biota) : base(biota)
        {
            if (Biota.TryRemoveProperty(PropertyBool.Open, BiotaDatabaseLock))
                ChangesDetected = true;

            // This is a temporary fix for objects that were loaded with this PR when EncumbranceVal was not treated as ephemeral. 2020-03-28
            // This can be removed later.
            if (Biota.PropertiesInt.ContainsKey(PropertyInt.EncumbranceVal))
            {
                var weenie = DatabaseManager.World.GetCachedWeenie(biota.WeenieClassId);

                if (weenie != null && weenie.PropertiesInt.TryGetValue(PropertyInt.EncumbranceVal, out var value))
                {
                    if (biota.PropertiesInt[PropertyInt.EncumbranceVal] != value)
                    {
                        biota.PropertiesInt[PropertyInt.EncumbranceVal] = value;
                        ChangesDetected = true;
                    }
                }
                else
                {
                    biota.PropertiesInt.Remove(PropertyInt.EncumbranceVal);
                    ChangesDetected = true;
                }
            }

            // This is a temporary fix for objects that were loaded with this PR when Value was not treated as ephemeral. 2020-03-28
            // This can be removed later.
            if (!(this is Creature) && Biota.PropertiesInt.ContainsKey(PropertyInt.Value))
            {
                var weenie = DatabaseManager.World.GetCachedWeenie(biota.WeenieClassId);

                if (weenie != null && weenie.PropertiesInt.TryGetValue(PropertyInt.Value, out var value))
                {
                    if (biota.PropertiesInt[PropertyInt.Value] != value)
                    {
                        biota.PropertiesInt[PropertyInt.Value] = value;
                        ChangesDetected = true;
                    }
                }
                else
                {
                    biota.PropertiesInt.Remove(PropertyInt.Value);
                    ChangesDetected = true;
                }
            }

            InitializePropertyDictionaries();
            SetEphemeralValues(true);

            // A player has their possessions passed via the ctor. All other world objects must load their own inventory
            if (!(this is Player) && !ObjectGuid.IsPlayer(ContainerId ?? 0))
            {
                DatabaseManager.Shard.GetInventoryInParallel(biota.Id, false, biotas =>
                {
                    //EnqueueAction(new ActionEventDelegate(() => SortBiotasIntoInventory(biotas)));
                    SortBiotasIntoInventory(biotas);
                });
            }
        }

        private void InitializePropertyDictionaries()
        {
            if (ephemeralPropertyInts == null)
                ephemeralPropertyInts = new Dictionary<PropertyInt, int?>();
        }

        private void SetEphemeralValues(bool fromBiota)
        {
            ephemeralPropertyInts.TryAdd(PropertyInt.EncumbranceVal, EncumbranceVal ?? 0); // Containers are init at 0 burden or their initial value from database. As inventory/equipment is added the burden will be increased
            if (!(this is Creature) && !(this is Corpse)) // Creatures/Corpses do not have a value
                ephemeralPropertyInts.TryAdd(PropertyInt.Value, Value ?? 0);

            //CurrentMotionState = motionStateClosed; // What container defaults to open?

            if (!fromBiota && !(this is Creature))
                GenerateContainList();

            if (!ContainerCapacity.HasValue)
                ContainerCapacity = 0;

            if (!UseRadius.HasValue)
                UseRadius = 0.5f;

            IsOpen = false;
        }

        public bool InventoryLoaded { get; private set; }

        /// <summary>
        /// This will contain all main pack items, and all side slot items.<para />
        /// To access items inside of the side slot items, you'll need to access that items.Inventory dictionary.<para />
        /// Do not manipulate this dictionary directly.
        /// </summary>
        public Dictionary<ObjectGuid, WorldObject> Inventory { get; } = new Dictionary<ObjectGuid, WorldObject>();

        /// <summary>
        /// The only time this should be used is to populate Inventory from the ctor.
        /// </summary>
        protected void SortBiotasIntoInventory(IEnumerable<ACE.Database.Models.Shard.Biota> biotas)
        {
            var worldObjects = new List<WorldObject>();

            foreach (var biota in biotas)
                worldObjects.Add(WorldObjectFactory.CreateWorldObject(biota));

            SortWorldObjectsIntoInventory(worldObjects);

            if (worldObjects.Count > 0)
                Console.WriteLine("Inventory detected without a container to put it in to.");
        }

        /// <summary>
        /// The only time this should be used is to populate Inventory from the ctor.
        /// This will remove from worldObjects as they're sorted.
        /// </summary>
        private void SortWorldObjectsIntoInventory(IList<WorldObject> worldObjects)
        {
            // This will pull out all of our main pack items and side slot items (foci & containers)
            for (int i = worldObjects.Count - 1; i >= 0; i--)
            {
                if ((worldObjects[i].ContainerId ?? 0) == Biota.Id)
                {
                    Inventory[worldObjects[i].Guid] = worldObjects[i];
                    worldObjects[i].Container = this;

                    if (worldObjects[i].WeenieType != WeenieType.Container) // We skip over containers because we'll add their burden/value in the next loop.
                    {
                        EncumbranceVal += (worldObjects[i].EncumbranceVal ?? 0);
                        Value += (worldObjects[i].Value ?? 0);
                    }

                    worldObjects.RemoveAt(i);
                }
            }

            // Make sure placement positions are correct. They could get out of sync from a client issue, server issue, or orphaned biota
            var mainPackItems = Inventory.Values.Where(wo => !wo.UseBackpackSlot).OrderBy(wo => wo.PlacementPosition).ToList();
            for (int i = 0; i < mainPackItems.Count; i++)
                mainPackItems[i].PlacementPosition = i;
            var sidPackItems = Inventory.Values.Where(wo => wo.UseBackpackSlot).OrderBy(wo => wo.PlacementPosition).ToList();
            for (int i = 0; i < sidPackItems.Count; i++)
                sidPackItems[i].PlacementPosition = i;

            InventoryLoaded = true;

            // All that should be left are side pack sub contents.

            var sideContainers = Inventory.Values.Where(i => i.WeenieType == WeenieType.Container).ToList();
            foreach (var container in sideContainers)
            {
                ((Container)container).SortWorldObjectsIntoInventory(worldObjects); // This will set the InventoryLoaded flag for this sideContainer
                EncumbranceVal += container.EncumbranceVal; // This value includes the containers burden itself + all child items
                Value += container.Value; // This value includes the containers value itself + all child items
            }

            OnInitialInventoryLoadCompleted();
        }

        public void GenerateContainList()
        {
            if (Biota.PropertiesCreateList == null)
                return;

            foreach (var item in Biota.PropertiesCreateList.Where(x => x.DestinationType == DestinationType.Contain || x.DestinationType == DestinationType.ContainTreasure))
            {
                var wo = WorldObjectFactory.CreateNewWorldObject(item.WeenieClassId);

                if (wo == null)
                    continue;

                if (item.Palette > 0)
                    wo.PaletteTemplate = item.Palette;
                if (item.Shade > 0)
                    wo.Shade = item.Shade;
                if (item.StackSize > 1)
                    wo.SetStackSize(item.StackSize);

                TryAddToInventory(wo);
            }
        }

        /// <summary>
        /// If enough burden is available, this will try to add an item to the main pack. If the main pack is full, it will try to add it to the first side pack with room.<para />
        /// It will also increase the EncumbranceVal and Value.
        /// </summary>
        public bool TryAddToInventory(WorldObject worldObject, int placementPosition = 0, bool limitToMainPackOnly = false, bool burdenCheck = true)
        {
            if (worldObject == null) return false;

            return TryAddToInventory(worldObject, out _, placementPosition, limitToMainPackOnly, burdenCheck);
        }

        /// <summary>
        /// If enough burden is available, this will try to add an item to the main pack. If the main pack is full, it will try to add it to the first side pack with room.<para />
        /// It will also increase the EncumbranceVal and Value.
        /// </summary>
        public bool TryAddToInventory(WorldObject worldObject, out Container container, int placementPosition = 0, bool limitToMainPackOnly = false, bool burdenCheck = true)
        {
            // bug: should be root owner
            /*if (this is Player player && burdenCheck)
            {
                if (!player.HasEnoughBurdenToAddToInventory(worldObject))
                {
                    container = null;
                    return false;
                }
            }*/

            IList<WorldObject> containerItems;

            if (worldObject.UseBackpackSlot)
            {
                containerItems = Inventory.Values.Where(i => i.UseBackpackSlot).ToList();

                if ((ContainerCapacity ?? 0) <= containerItems.Count)
                {
                    container = null;
                    return false;
                }
            }
            else
            {
                containerItems = Inventory.Values.Where(i => !i.UseBackpackSlot).ToList();

                if ((ItemCapacity ?? 0) <= containerItems.Count)
                {
                    // Can we add this to any side pack?
                    if (!limitToMainPackOnly)
                    {
                        var containers = Inventory.Values.OfType<Container>().ToList();
                        containers.Sort((a, b) => (a.Placement ?? 0).CompareTo(b.Placement ?? 0));

                        foreach (var sidePack in containers)
                        {
                            if (sidePack.TryAddToInventory(worldObject, out container, placementPosition, true))
                            {
                                EncumbranceVal += (worldObject.EncumbranceVal ?? 0);
                                Value += (worldObject.Value ?? 0);

                                return true;
                            }
                        }
                    }

                    container = null;
                    return false;
                }
            }

            if (Inventory.ContainsKey(worldObject.Guid))
            {
                container = null;
                return false;
            }

            worldObject.Location = null;
            worldObject.Placement = ACE.Entity.Enum.Placement.Resting;

            worldObject.OwnerId = Guid.Full;
            worldObject.ContainerId = Guid.Full;
            worldObject.Container = this;
            worldObject.PlacementPosition = placementPosition; // Server only variable that we use to remember/restore the order in which items exist in a container

            // Move all the existing items PlacementPosition over.
            if (!worldObject.UseBackpackSlot)
                containerItems.Where(i => !i.UseBackpackSlot && i.PlacementPosition >= placementPosition).ToList().ForEach(i => i.PlacementPosition++);
            else
                containerItems.Where(i => i.UseBackpackSlot && i.PlacementPosition >= placementPosition).ToList().ForEach(i => i.PlacementPosition++);

            Inventory.Add(worldObject.Guid, worldObject);

            EncumbranceVal += (worldObject.EncumbranceVal ?? 0);
            Value += (worldObject.Value ?? 0);

            container = this;

            OnAddItem();

            return true;
        }

        /// <summary>
        /// This event is raised after the containers items have been completely loaded from the database
        /// </summary>
        protected virtual void OnInitialInventoryLoadCompleted()
        {
            // empty base
        }

        /// <summary>
        /// This event is raised when player adds item to container
        /// </summary>
        protected virtual void OnAddItem()
        {
            // empty base
        }

        /// <summary>
        /// This event is raised when player removes item from container
        /// </summary>
        protected virtual void OnRemoveItem(WorldObject worldObject)
        {
            // empty base
        }

        /// <summary>
        /// This method is used to get all inventory items of a type in this container (example of usage get all items of coin on player)
        /// </summary>
        public List<WorldObject> GetInventoryItemsOfTypeWeenieType(WeenieType type)
        {
            var items = new List<WorldObject>();

            // first search me / add all items of type.
            var localInventory = Inventory.Values.Where(wo => wo.WeenieType == type).OrderBy(i => i.PlacementPosition).ToList();

            items.AddRange(localInventory);

            // next search all containers for type.. run function again for each container.
            var sideContainers = Inventory.Values.Where(i => i.WeenieType == WeenieType.Container).OrderBy(i => i.PlacementPosition).ToList();
            foreach (var container in sideContainers)
                items.AddRange(((Container)container).GetInventoryItemsOfTypeWeenieType(type));

            return items;
        }

        /// <summary>
        /// This will clear the ContainerId and PlacementPosition properties.<para />
        /// It will also subtract the EncumbranceVal and Value.
        /// </summary>
        public bool TryRemoveFromInventory(ObjectGuid objectGuid, bool forceSave = false)
        {
            return TryRemoveFromInventory(objectGuid, out _, forceSave);
        }

        /// <summary>
        /// This will clear the ContainerId and PlacementPosition properties and remove the object from the Inventory dictionary.<para />
        /// It will also subtract the EncumbranceVal and Value.
        /// </summary>
        public bool TryRemoveFromInventory(ObjectGuid objectGuid, out WorldObject item, bool forceSave = false)
        {
            // first search me / add all items of type.
            if (Inventory.Remove(objectGuid, out item))
            {
                int removedItemsPlacementPosition = item.PlacementPosition ?? 0;

                item.OwnerId = null;
                item.ContainerId = null;
                item.Container = null;
                item.PlacementPosition = null;

                // Move all the existing items PlacementPosition over.
                if (!item.UseBackpackSlot)
                    Inventory.Values.Where(i => !i.UseBackpackSlot && i.PlacementPosition > removedItemsPlacementPosition).ToList().ForEach(i => i.PlacementPosition--);
                else
                    Inventory.Values.Where(i => i.UseBackpackSlot && i.PlacementPosition > removedItemsPlacementPosition).ToList().ForEach(i => i.PlacementPosition--);

                EncumbranceVal -= (item.EncumbranceVal ?? 0);
                Value -= (item.Value ?? 0);

                //if (forceSave)
                    //item.SaveBiotaToDatabase();

                OnRemoveItem(item);

                return true;
            }

            // next search all containers for item.. run function again for each container.
            var sideContainers = Inventory.Values.Where(i => i.WeenieType == WeenieType.Container).ToList();
            foreach (var container in sideContainers)
            {
                if (((Container)container).TryRemoveFromInventory(objectGuid, out item))
                {
                    EncumbranceVal -= (item.EncumbranceVal ?? 0);
                    Value -= (item.Value ?? 0);

                    return true;
                }
            }

            return false;
        }
    }
}

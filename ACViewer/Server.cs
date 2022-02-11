using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using ACE.Common;
using ACE.Database;
using ACE.Database.Models.World;
using ACE.Entity.Enum;
using ACE.Server.Factories;
using ACE.Server.Physics;
using ACE.Server.Physics.Animation;
using ACE.Server.Physics.Common;
using ACE.Server.WorldObjects;

using ACViewer.Enum;
using ACViewer.Model;
using ACViewer.Render;
using ACViewer.View;

namespace ACViewer
{
    public static class Server
    {
        public static Config.Config Config => ACViewer.Config.ConfigManager.Config;

        public static Render.Buffer Buffer => GameView.Instance.Render.Buffer;

        public static List<WorldObject> UpdateObjs { get; set; }

        public static bool Initting { get; set; }

        public static bool InstancesLoaded { get; set; }

        public static bool EncountersLoaded { get; set; }

        public static List<WorldObject> Instances { get; set; }

        public static List<WorldObject> Encounters { get; set; }

        public static void Init()
        {
            InstancesLoaded = false;
            EncountersLoaded = false;
        }
        
        public static void LoadInstances()
        {
            Initting = true;
            MainWindow.Instance.SuppressStatusText = true;
            
            SetDatabaseConfig();

            Instances = new List<WorldObject>();

            var timer = Stopwatch.StartNew();
            
            // get the list of loaded landblocks
            foreach (var lbid in LScape.Landblocks.Keys)
            {
                var instances = DatabaseManager.World.GetCachedInstancesByLandblock((ushort)(lbid >> 16));

                // build lookup table
                var lookupTable = instances.ToDictionary(i => i.Guid, i => i);

                Console.WriteLine($"Found {instances.Count:N0} instances for {lbid:X8}");

                foreach (var instance in instances)
                {
                    if (instance.IsLinkChild) continue;

                    var wo = ProcessInstance(lbid, instance, lookupTable);

                    if (wo != null)
                        ActivateLinks(wo);
                }
            }

            TickGenerators(GeneratorTickMode.Instances);

            timer.Stop();

            Console.WriteLine($"Completed in {timer.Elapsed.TotalSeconds}s");
        }

        public static void ActivateLinks(WorldObject wo)
        {
            foreach (var child in wo.ChildLinks)
            {
                wo.SetLinkProperties(child);
                ActivateLinks(child);
            }
        }

        public static WorldObject ProcessInstance(uint lbid, LandblockInstance instance, Dictionary<uint, LandblockInstance> lookupTable)
        {
            var wo = WorldObjectFactory.CreateNewWorldObject(instance.WeenieClassId);

            if (wo == null) return null;

            wo.InitPhysicsObj();

            var objCellId = instance.ObjCellId;

            if ((objCellId & 0xFFFF) == 0)
            {
                // get the outdoor landcell for this position
                var cellX = (uint)(instance.OriginX / 24);
                var cellY = (uint)(instance.OriginY / 24);

                var cellID = cellX * 8 + cellY + 1;

                objCellId |= cellID;
            }

            var location = new Position();
            location.ObjCellID = objCellId;
            location.Frame.Origin = new Vector3(instance.OriginX, instance.OriginY, instance.OriginZ);
            location.Frame.Orientation = new Quaternion(instance.AnglesX, instance.AnglesY, instance.AnglesZ, instance.AnglesW);

            var success = wo.AddPhysicsObj(location);

            if (!success)
            {
                Console.WriteLine($"LoadInstances({lbid:X8}).AddPhysicsObj({wo.Name}, {location}) - failed to spawn");
                return null;
            }

            Console.WriteLine($"Spawned {instance.WeenieClassId} - {wo.Name} @ {location}");

            AddInstance(wo);

            foreach (var link in instance.LandblockInstanceLink)
            {
                if (!lookupTable.TryGetValue(link.ChildGuid, out var childInstance))
                {
                    Console.WriteLine($"Server.ProcessInstance({lbid:X8}, {instance.Guid:X8}, {link.ChildGuid:X8}) - couldn't find child guid!");
                    continue;
                }

                var child = ProcessInstance(lbid, childInstance, lookupTable);

                if (child == null) continue;

                child.ParentLink = wo;
                wo.ChildLinks.Add(child);
            }

            return wo;
        }

        public static void AddInstance(WorldObject wo)
        {
            Instances.Add(wo);

            var objDesc = new ObjDesc(wo.SetupTableId, wo.ClothingBase ?? 0, (PaletteTemplate)(wo.PaletteTemplate ?? 0), (float)(wo.Shade ?? 0.0));

            if (wo is Creature creature)
            {
                objDesc.AddBaseModelData(wo);

                var equippedObjects = creature.EquippedObjects.Values.OrderBy(i => (int)(i.ClothingPriority ?? 0)).ToList();

                foreach (var equippedObject in equippedObjects)
                {
                    if ((equippedObject.CurrentWieldedLocation & EquipMask.Selectable) != 0)
                        continue;

                    objDesc.Add(equippedObject.ClothingBase ?? 0, (PaletteTemplate)(equippedObject.PaletteTemplate ?? 0), (float)(equippedObject.Shade ?? 0.0));
                }
            }

            wo.PhysicsObj.UpdateObjDesc(objDesc);

            var r_PhysicsObj = new R_PhysicsObj(wo.PhysicsObj);
            Buffer.AddInstance(r_PhysicsObj, objDesc);

            if (UpdateObjs == null)
                UpdateObjs = new List<WorldObject>();

            UpdateObjs.Add(wo);
        }

        public static void LoadInstances_Finalize()
        {
            Buffer.BuildTextureAtlases(Buffer.InstanceTextureAtlasChains);
            Buffer.BuildBuffer(Buffer.RB_Instances);

            if (MainMenu.ShowParticles)
            {
                // todo: optimize
                GameView.Instance.Render.DestroyEmitters();
                GameView.Instance.Render.InitEmitters();
            }

            MainWindow.Instance.SuppressStatusText = false;
            InstancesLoaded = true;
            Initting = false;
        }

        public static void LoadEncounters()
        {
            Initting = true;
            MainWindow.Instance.SuppressStatusText = true;

            SetDatabaseConfig();

            Encounters = new List<WorldObject>();

            var timer = Stopwatch.StartNew();

            // get the list of loaded landblocks
            foreach (var lbid in LScape.Landblocks.Keys)
            {
                var encounters = DatabaseManager.World.GetCachedEncountersByLandblock((ushort)(lbid >> 16));

                Console.WriteLine($"Found {encounters.Count:N0} encounters for {lbid:X8}");

                var landblock = LScape.get_landblock(lbid);
                
                foreach (var encounter in encounters)
                {
                    var wo = WorldObjectFactory.CreateNewWorldObject(encounter.WeenieClassId);

                    if (wo == null) continue;

                    wo.InitPhysicsObj();

                    var xPos = Math.Clamp(encounter.CellX * 24.0f, 0.5f, 191.5f);
                    var yPos = Math.Clamp(encounter.CellY * 24.0f, 0.5f, 191.5f);

                    var pos = new Position();
                    pos.ObjCellID = lbid & 0xFFFF0000 | 1;
                    pos.Frame = new AFrame(new Vector3(xPos, yPos, 0), Quaternion.Identity);
                    pos.adjust_to_outside();

                    pos.Frame.Origin.Z = landblock.GetZ(pos.Frame.Origin);

                    wo.Location = new ACE.Entity.Position(pos.ObjCellID, pos.Frame.Origin, pos.Frame.Orientation);

                    var sortCell = LScape.get_landcell(pos.ObjCellID) as SortCell;
                    if (sortCell != null && sortCell.has_building())
                        continue;

                    var success = wo.AddPhysicsObj(pos);
                    
                    if (!success)
                    {
                        Console.WriteLine($"LoadEncounters({lbid:X8}).AddPhysicsObj({wo.Name}, {pos}) - failed to spawn");
                        continue;
                    }

                    Console.WriteLine($"Spawned {encounter.WeenieClassId} - {wo.Name} @ {pos}");

                    AddEncounter(wo);
                }
            }

            TickGenerators(GeneratorTickMode.Encounters);

            timer.Stop();

            Console.WriteLine($"Completed in {timer.Elapsed.TotalSeconds}s");
        }

        public static void AddEncounter(WorldObject wo)
        {
            Encounters.Add(wo);

            var objDesc = new ObjDesc(wo.SetupTableId, wo.ClothingBase ?? 0, (PaletteTemplate)(wo.PaletteTemplate ?? 0), (float)(wo.Shade ?? 0.0));

            if (wo is Creature creature)
            {
                foreach (var equippedObject in creature.EquippedObjects.Values.OrderBy(i => (int)(i.ClothingPriority ?? 0)))
                {
                    if ((equippedObject.CurrentWieldedLocation & EquipMask.Selectable) != 0)
                        continue;

                    objDesc.Add(equippedObject.ClothingBase ?? 0, (PaletteTemplate)(equippedObject.PaletteTemplate ?? 0), (float)(equippedObject.Shade ?? 0.0));
                }
            }

            wo.PhysicsObj.UpdateObjDesc(objDesc);

            var r_PhysicsObj = new R_PhysicsObj(wo.PhysicsObj);
            Buffer.AddEncounter(r_PhysicsObj, objDesc);

            if (UpdateObjs == null)
                UpdateObjs = new List<WorldObject>();

            UpdateObjs.Add(wo);
        }

        public static void LoadEncounters_Finalize()
        {
            Buffer.BuildTextureAtlases();
            Buffer.BuildBuffer(Buffer.RB_Encounters);

            if (MainMenu.ShowParticles)
            {
                // todo: optimize
                GameView.Instance.Render.DestroyEmitters();
                GameView.Instance.Render.InitEmitters();
            }

            MainWindow.Instance.SuppressStatusText = false;
            EncountersLoaded = true;
            Initting = false;
        }

        public static void SetDatabaseConfig()
        {
            var masterConfig = new MasterConfiguration();
            masterConfig.MySql = new DatabaseConfiguration();
            masterConfig.MySql.World = new MySqlConfiguration();

            masterConfig.MySql.World.Host = Config.Database.Host;
            masterConfig.MySql.World.Port = (uint)Config.Database.Port;
            masterConfig.MySql.World.Database = Config.Database.DatabaseName;
            masterConfig.MySql.World.Username = Config.Database.Username;
            masterConfig.MySql.World.Password = Config.Database.Password;

            ConfigManager.Initialize(masterConfig);
        }

        public static void Update()
        {
            if (UpdateObjs == null || Initting) return;

            for (var i = UpdateObjs.Count - 1; i >= 0; i--)
            {
                var updateObj = UpdateObjs[i];

                updateObj.PhysicsObj.update_object();

                if (updateObj.PhysicsObj.InitialUpdates > 1 || !updateObj.PhysicsObj.TransientState.HasFlag(TransientStateFlags.Active) || updateObj.PhysicsObj.IsDestroyed)
                    UpdateObjs.RemoveAt(i);
            }
        }

        public static void TryPrimeDatabase()
        {
            SetDatabaseConfig();

            var worker = new BackgroundWorker();

            worker.DoWork += (sender, doWorkEventArgs) =>
            {
                try
                {
                    using (var ctx = new WorldDbContext())
                    {
                        var weenie = ctx.Weenie.FirstOrDefault();
                    }
                }
                catch (Exception)
                {
                }
            };
            worker.RunWorkerAsync();
        }

        public static void ClearInstances()
        {
            if (!InstancesLoaded) return;

            foreach (var instance in Instances)
                instance.PhysicsObj.DestroyObject();

            Buffer.ClearBuffer(Buffer.RB_Instances);
            Buffer.RB_Instances = new Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared>();

            foreach (var textureAtlasChain in Buffer.InstanceTextureAtlasChains.Values)
                textureAtlasChain.Dispose();

            Buffer.InstanceTextureAtlasChains.Clear();

            InstancesLoaded = false;
            Instances = null;
        }

        public static void ClearEncounters()
        {
            if (!EncountersLoaded) return;

            foreach (var encounter in Encounters)
                encounter.PhysicsObj.DestroyObject();

            Buffer.ClearBuffer(Buffer.RB_Encounters);
            Buffer.RB_Encounters = new Dictionary<GfxObjTexturePalette, GfxObjInstance_Shared>();

            EncountersLoaded = false;
            Encounters = null;
        }

        public static GeneratorTickMode GeneratorTickMode { get; set; }
        
        public static void TickGenerators(GeneratorTickMode generatorTickMode)
        {
            if (UpdateObjs == null) return;
            
            GeneratorTickMode = generatorTickMode;
            
            for (var i = 0; i < UpdateObjs.Count; i++)
            {
                var updateObj = UpdateObjs[i];
                
                if (!updateObj.IsGenerator) continue;

                updateObj.Generator_Update();

                updateObj.Generator_Regeneration();
            }

            GeneratorTickMode = GeneratorTickMode.Undef;
        }

        public static bool EnterWorld(WorldObject wo)
        {
            // called by generators
            wo.InitPhysicsObj();
            
            var success = wo.AddPhysicsObj(new Position(wo.Location));

            if (success)
            {
                switch (GeneratorTickMode)
                {
                    case GeneratorTickMode.Instances:
                        AddInstance(wo);
                        break;

                    case GeneratorTickMode.Encounters:
                        AddEncounter(wo);
                        break;

                    default:
                        Console.WriteLine($"Server.EnterWorld({wo.WeenieClassId} - {wo.Name}) - GeneratorTickMode.{GeneratorTickMode}");
                        break;
                }
            }

            return success;
        }

        public static void OnLoadWorld()
        {
            if (!MainMenu.LoadInstances && !MainMenu.LoadEncounters)
                return;

            var worker = new BackgroundWorker();

            if (MainMenu.LoadInstances)
            {
                worker.DoWork += (sender, doWorkEventArgs) => LoadInstances();
                worker.RunWorkerCompleted += (sender, runWorkerCompletedEventArgs) => LoadInstances_Finalize();
            }

            if (MainMenu.LoadEncounters)
            {
                worker.DoWork += (sender, doWorkEventArgs) => LoadEncounters();
                worker.RunWorkerCompleted += (sender, runWorkerCompletedEventArgs) => LoadEncounters_Finalize();
            }

            worker.RunWorkerAsync();
        }
    }
}

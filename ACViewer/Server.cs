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

using ACViewer.Model;
using ACViewer.Render;
using ACViewer.View;

namespace ACViewer
{
    public static class Server
    {
        public static Config.Config Config => ACViewer.Config.ConfigManager.Config;

        public static Render.Buffer Buffer => GameView.Instance.Render.Buffer;

        public static List<PhysicsObj> UpdateObjs { get; set; }

        public static bool Initting { get; set; }

        public static bool InstancesLoaded { get; set; }

        public static bool EncountersLoaded { get; set; }

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

            var timer = Stopwatch.StartNew();
            
            // get the list of loaded landblocks
            foreach (var lbid in LScape.Landblocks.Keys)
            {
                var instances = DatabaseManager.World.GetCachedInstancesByLandblock((ushort)(lbid >> 16));

                Console.WriteLine($"Found {instances.Count:N0} instances for {lbid:X8}");

                /*Parallel.ForEach(instances, instance =>
                {
                });*/
                
                foreach (var instance in instances)
                {
                    var wo = WorldObjectFactory.CreateNewWorldObject(instance.WeenieClassId);

                    if (wo == null) continue;

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

                    WorldObject.AdjustDungeonCells(location);

                    var success = wo.PhysicsObj.enter_world(location);

                    if (!success || wo.PhysicsObj.CurCell == null)
                    {
                        wo.PhysicsObj.DestroyObject();
                        wo.PhysicsObj = null;
                        Console.WriteLine($"LoadInstances({lbid:X8}).AddPhysicsObj({wo.Name}, {location}) - failed to spawn");
                        continue;
                    }

                    Console.WriteLine($"Spawned {instance.WeenieClassId} - {wo.Name} @ {location}");

                    var objDesc = new ObjDesc(wo.SetupTableId, wo.ClothingBase ?? 0, (PaletteTemplate)(wo.PaletteTemplate ?? 0), (float)(wo.Shade ?? 0.0));

                    if (wo is Creature creature)
                    {
                        var equippedObjects = creature.EquippedObjects.Values.OrderBy(i => (int)(i.ClothingPriority ?? 0)).ToList();

                        foreach (var equippedObject in equippedObjects)
                        {
                            if ((equippedObject.CurrentWieldedLocation & EquipMask.Selectable) != 0)
                                continue;

                            objDesc.Add(wo.SetupTableId, equippedObject.ClothingBase ?? 0, (PaletteTemplate)(equippedObject.PaletteTemplate ?? 0), (float)(equippedObject.Shade ?? 0.0));
                        }
                    }

                    wo.PhysicsObj.UpdateObjDesc(objDesc);
                    
                    var r_PhysicsObj = new R_PhysicsObj(wo.PhysicsObj);
                    Buffer.AddInstance(r_PhysicsObj, objDesc);

                    if (UpdateObjs == null)
                        UpdateObjs = new List<PhysicsObj>();

                    UpdateObjs.Add(wo.PhysicsObj);
                }
            }

            timer.Stop();

            Console.WriteLine($"Completed in {timer.Elapsed.TotalSeconds}s");
        }

        public static void LoadInstances_Finalize()
        {
            Buffer.BuildTextureAtlases();
            Buffer.BuildBuffer(Buffer.RB_Instances);

            MainWindow.Instance.SuppressStatusText = false;
            InstancesLoaded = true;
            Initting = false;
        }

        public static void LoadEncounters()
        {
            Initting = true;
            MainWindow.Instance.SuppressStatusText = true;

            SetDatabaseConfig();

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

                    var success = wo.PhysicsObj.enter_world(pos);

                    if (!success || wo.PhysicsObj.CurCell == null)
                    {
                        wo.PhysicsObj.DestroyObject();
                        wo.PhysicsObj = null;
                        Console.WriteLine($"LoadEncounters({lbid:X8}).AddPhysicsObj({wo.Name}, {pos}) - failed to spawn");
                        continue;
                    }

                    Console.WriteLine($"Spawned {encounter.WeenieClassId} - {wo.Name} @ {pos}");

                    var objDesc = new ObjDesc(wo.SetupTableId, wo.ClothingBase ?? 0, (PaletteTemplate)(wo.PaletteTemplate ?? 0), (float)(wo.Shade ?? 0.0));

                    if (wo is Creature creature)
                    {
                        foreach (var equippedObject in creature.EquippedObjects.Values.OrderBy(i => (int)(i.ClothingPriority ?? 0)))
                        {
                            if ((equippedObject.CurrentWieldedLocation & EquipMask.Selectable) != 0)
                                continue;

                            objDesc.Add(wo.SetupTableId, equippedObject.ClothingBase ?? 0, (PaletteTemplate)(equippedObject.PaletteTemplate ?? 0), (float)(equippedObject.Shade ?? 0.0));
                        }
                    }

                    wo.PhysicsObj.UpdateObjDesc(objDesc);

                    var r_PhysicsObj = new R_PhysicsObj(wo.PhysicsObj);
                    Buffer.AddEncounter(r_PhysicsObj, objDesc);

                    if (UpdateObjs == null)
                        UpdateObjs = new List<PhysicsObj>();

                    UpdateObjs.Add(wo.PhysicsObj);
                }
            }

            timer.Stop();

            Console.WriteLine($"Completed in {timer.Elapsed.TotalSeconds}s");
        }

        public static void LoadEncounters_Finalize()
        {
            Buffer.BuildTextureAtlases();
            Buffer.BuildBuffer(Buffer.RB_Encounters);

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

                updateObj.update_object();

                if (updateObj.InitialUpdates > 1)
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
    }
}

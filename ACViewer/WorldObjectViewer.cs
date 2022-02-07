using System;
using System.Linq;

using Microsoft.Xna.Framework;

using ACE.Entity.Enum;
using ACE.Server.Factories;
using ACE.Server.Physics.Common;
using ACE.Server.WorldObjects;

using ACViewer.Model;
using ACViewer.Render;

namespace ACViewer
{
    public class WorldObjectViewer
    {
        public static WorldObjectViewer Instance { get; set; }

        public static Camera Camera => GameView.Camera;

        public bool GfxObjMode { get; set; }

        public static Render.Buffer Buffer => GameView.Instance.Render.Buffer;

        public WorldObjectViewer()
        {
            Instance = this;
        }

        public static bool LoadedOnce;

        public void LoadModel(uint wcid)
        {
            Server.Initting = true;

            Buffer.ClearBuffer();
            TextureCache.Init();

            LScape.unload_landblocks_all();

            Console.WriteLine($"Loading {wcid}");
            GfxObjMode = false;

            var wo = WorldObjectFactory.CreateNewWorldObject(wcid);

            if (wo == null) return;

            wo.InitPhysicsObj();

            var location = new Position();
            location.ObjCellID = 0x00000001;
            location.Frame.Origin = new System.Numerics.Vector3(12, 12, 0);

            var success = wo.PhysicsObj.enter_world(location);

            if (!success || wo.PhysicsObj.CurCell == null)
            {
                wo.PhysicsObj.DestroyObject();
                wo.PhysicsObj = null;
                Console.WriteLine($"WorldObjectViewer.LoadModel({wcid}).AddPhysicsObj({wo.Name}, {location}) - failed to spawn");
                return;
            }

            Console.WriteLine($"Spawned {wcid} - {wo.Name} @ {location}");

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

            if (!LoadedOnce)
            {
                //Camera.Position = Vector3.Zero;
                //Camera.Dir = Vector3.Normalize(new Vector3(1, 1, 0));
                
                Camera.Position = new Vector3(11.782367f, 12.763985f, 1.6514041f);
                Camera.Dir = new Vector3(0.30761153f, -0.94673103f, 0.093334414f);
                Camera.Up = Vector3.UnitZ;

                Camera.CreateLookAt();

                Camera.Speed = Camera.Model_Speed;
            }

            Buffer.BuildTextureAtlases(Buffer.InstanceTextureAtlasChains);
            Buffer.BuildBuffer(Buffer.RB_Instances);
            Server.InstancesLoaded = true;
            Server.Initting = false;

            LoadedOnce = true;
        }

        public void Update(GameTime time)
        {
            if (Camera != null)
                Camera.Update(time);
        }

        public void Draw(GameTime time)
        {
            GameView.Instance.Render.Draw();
            GameView.Instance.Render.DrawHUD();
        }
    }
}


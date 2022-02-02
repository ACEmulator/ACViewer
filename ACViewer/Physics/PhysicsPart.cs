using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using ACE.Entity.Enum;
using ACE.Server.Physics.Animation;
using ACE.Server.Physics.Common;
using ACE.Server.Physics.Collision;
using ACE.Server.Physics.Entity;

using ACViewer;
using ACViewer.Enum;
using ACViewer.Render;

namespace ACE.Server.Physics
{
    /// <summary>
    /// A static GfxObj from the DAT, combined with dynamic info at runtime (position/rotation/scale)
    /// </summary>
    public class PhysicsPart
    {
        //public float CYpt;
        //public Vector3 ViewerHeading;
        //public DatLoader.FileTypes.GfxObjDegradeInfo Degrades;
        //public int DegLevel;
        //public int DegMode;
        public GfxObj GfxObj;
        public Vector3 GfxObjScale;
        public Position Pos;        // this contains all of the per-part, per-frame animation data
        //public Position DrawPos;
        //public Material Material;
        //public List<uint> Surfaces;
        //public int OriginalPaletteID;
        public float CurTranslucency;
        //public float CurDiffuse;
        //public float CurLuminosity;
        //public DatLoader.FileTypes.Palette ShiftPal;
        //public int CurrentRenderFrameNum;
        public PhysicsObj PhysicsObj;
        public int PhysObjIndex;
        public BBox BoundingBox;
        public bool NoDraw;

        public static PhysicsObj PlayerObject;

        // acviewer custom
        public GfxObjInstance_Shared Buffer { get; set; }
        public int BufferIdx { get; set; }

        public PhysicsPart()
        {
            InitEmpty();
        }

        public PhysicsPart(uint partID)
        {
            InitEmpty();
            SetPart(partID);
        }

        public TransitionState FindObjCollisions(Transition transition, int partIdx)
        {
            if (GfxObj != null && GfxObj.PhysicsBSP != null)
            {
                transition.SpherePath.CacheLocalSpaceSphere(Pos, GfxObjScale.Z);
                
                var result = GfxObj.FindObjCollisions(transition, GfxObjScale.Z);

                if (result == TransitionState.OK || !PhysicsObj.IsPicking)
                    return result;

                return FindObjCollisions_Draw(transition, partIdx);
            }
            return TransitionState.OK;  // should be invalid?
        }

        public int NextPolyIdx;

        public TransitionState FindObjCollisions_Draw(Transition transition, int partIdx)
        {
            if (GfxObj == null || GfxObj.DrawingBSP == null)
                return TransitionState.OK;
            
            transition.SpherePath.CacheLocalSpaceSphere(Pos, GfxObjScale.Z);
                
            var result = GfxObj.FindObjCollisions_Draw(transition, GfxObjScale.Z);

            if (result == TransitionState.OK) return result;

            // test against each poly in gfxobj
            // build a ray for more precision
            var dir = Picker.Dir.ToNumerics();
            var q = Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Pos.Frame.Orientation));  // invert
            dir = Vector3.Transform(dir, q);
            var ray = new Ray(transition.SpherePath.LocalSpaceSphere[0].Center, dir, GfxObj.DrawingBSP.RootNode.Sphere.Radius * GfxObjScale.Z);

            var hitPolys = new Dictionary<ushort, float>(); // polyIdx, contactTime

            foreach (var polygon in GfxObj.Polygons)
            {
                var contactTime = 0.0f;

                if (polygon.Value.polygon_hits_ray(ray, ref contactTime))
                {
                    hitPolys.Add(polygon.Key, contactTime);
                }
            }

            if (hitPolys.Count == 0)
            {
                // at this point, we have a less precise sphere collision, but no precise ray collisions. no true collision detected
                return TransitionState.OK;
            }

            Picker.PickResult.Type = PickType.GfxObj;
            Picker.PickResult.PhysicsObj = PhysicsObj;
            Picker.PickResult.PartIdx = partIdx;

            // sort by contactTime, closest first
            var closestPolyIdx = hitPolys.OrderBy(i => i.Value).Select(i => i.Key).First();

            Picker.PickResult.PolyIdx = closestPolyIdx;

            PhysicsObj.IsPicking = false;

            return TransitionState.Collided;
        }

        public BBox GetBoundingBox()
        {
            return GfxObj.GfxBoundBox;
        }

        public uint GetPhysObjID()
        {
            if (PhysicsObj == null)
                return 0;

            return PhysicsObj.ID;
        }

        public void InitEmpty()
        {
            GfxObjScale = new Vector3(1.0f, 1.0f, 1.0f);
            Pos = new Position();
            //DrawPos = new Position();
            //ViewerHeading = new Vector3(0.0f, 0.0f, 1.0f);
            PhysObjIndex = -1;
            //DegMode = 1;
            //CYpt = Int16.MaxValue;
        }

        public bool InitObjDescChanges()
        {
            return false;
        }

        public bool IsPartOfPlayerObj()
        {
            return PhysicsObj.Equals(PlayerObject);
        }

        public bool LoadGfxObjArray(uint rootObjectID/*, GfxObjDegradeInfo newDegrades*/)
        {
            GfxObj = GfxObjCache.Get(rootObjectID);
            // degrades omitted
            return GfxObj != null;
        }

        public static PhysicsPart MakePhysicsPart(uint gfxObjID)
        {
            return new PhysicsPart(gfxObjID);
        }

        public static PhysicsPart MakePhysicsPart(PhysicsPart template)
        {
            var part = new PhysicsPart();
            part.MorphToExistingObject(template);
            return part;
        }

        public bool MorphToExistingObject(PhysicsPart template)
        {
            // copy constructor?
            GfxObj = template.GfxObj;   
            GfxObjScale = template.GfxObjScale;
            Pos = template.Pos;
            //DrawPos = template.DrawPos;
            //OriginalPaletteID = template.OriginalPaletteID;
            // removed surfaces
            return true;
        }

        public void SetNoDraw(bool noDraw)
        {
            // graphics omitted from server
        }

        public bool SetPart(uint gfxObjID)
        {
            return LoadGfxObjArray(gfxObjID);
        }

        public void SetTranslucency(float translucency)
        {
            if (PhysicsObj != null && PhysicsObj.State.HasFlag(PhysicsState.Cloaked))
                return;

            if (translucency == 1.0f)
            {
                NoDraw = true;
                return;
            }

            if (CurTranslucency != translucency)
            {
                CurTranslucency = translucency;
                /*if (CurSettingsAreDefault())
                    RestoreMaterial();
                else if (CopyMaterial())
                    Material.SetTranslucencySimple(translucency);*/
            }
        }

        public void UpdateViewerDistance()
        {
            // client rendering?
        }
    }
}

using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;

namespace ACViewer.Model
{
    public class Setup
    {
        public SetupModel _setup { get; set; }

        public List<GfxObj> Parts { get; set; }

        public List<Matrix> PlacementFrames { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public Setup() { }

        public Setup(uint setupID)
        {
            // make simple setup if gfxobj
            if (setupID >> 24 == 0x1)
            {
                MakeSimpleSetup(setupID);
                BuildBoundingBox();
                return;
            }

            _setup = DatManager.PortalDat.ReadFromDat<SetupModel>(setupID);

            Parts = new List<GfxObj>();

            foreach (var part in _setup.Parts)
                Parts.Add(GfxObjCache.Get(part));

            PlacementFrames = new List<Matrix>();

            if (!_setup.PlacementFrames.TryGetValue((int)Placement.Resting, out var placementFrames))
                _setup.PlacementFrames.TryGetValue((int)Placement.Default, out placementFrames);

            if (placementFrames != null)
            {
                foreach (var placementFrame in placementFrames.AnimFrame.Frames)
                    PlacementFrames.Add(placementFrame.ToXna());
            }

            BuildBoundingBox();
        }

        public Setup(uint setupID, ObjDesc objDesc)
        {
            // make simple setup if gfxobj. These don't have 
            if (setupID >> 24 == 0x1)
            {
                MakeSimpleSetup(setupID);
                BuildBoundingBox();
                return;
            }
            _setup = DatManager.PortalDat.ReadFromDat<SetupModel>(setupID);

            Parts = new List<GfxObj>();

            for (var i = 0; i < _setup.Parts.Count; i++)
            {
                GfxObj gfxObj;

                if (objDesc.PartChanges != null && objDesc.PartChanges.TryGetValue((uint)i, out var partChange))
                {
                    gfxObj = new GfxObj(partChange.NewGfxObjId, false);

                    gfxObj.LoadTextures(partChange.TextureChanges, objDesc.PaletteChanges);

                    gfxObj.BuildPolygons();
                }
                else
                {
                    var gfxObjID = _setup.Parts[i];
                    gfxObj = new GfxObj(gfxObjID, false);

                    gfxObj.LoadTextures(null, objDesc.PaletteChanges);

                    gfxObj.BuildPolygons();
                }

                Parts.Add(gfxObj);
            }

            PlacementFrames = new List<Matrix>();

            if (!_setup.PlacementFrames.TryGetValue((int)Placement.Resting, out var placementFrames))
                _setup.PlacementFrames.TryGetValue((int)Placement.Default, out placementFrames);

            foreach (var placementFrame in placementFrames.AnimFrame.Frames)
                PlacementFrames.Add(placementFrame.ToXna());

            BuildBoundingBox();
        }

        public void MakeSimpleSetup(uint gfxObjID)
        {
            _setup = SetupModel.CreateSimpleSetup();

            Parts = new List<GfxObj>(1);

            var gfxObj = GfxObjCache.Get(gfxObjID);
            Parts.Add(gfxObj);

            // always identity?
            PlacementFrames = new List<Matrix>(1);
            PlacementFrames.Add(Matrix.Identity);
        }

        public List<Vector3> GetVertices()
        {
            var verts = new List<Vector3>();

            for (var i = 0; i < Parts.Count; i++)
            {
                if (Parts[i]._gfxObj.Id == 0x010001EC)
                    continue;

                var part = Parts[i];

                var placementFrame = Matrix.Identity;

                if (i < PlacementFrames.Count)
                    placementFrame = PlacementFrames[i];

                var partVerts = part.VertexArray.Select(v => v.Position).ToList();

                foreach (var partVert in partVerts)
                    verts.Add(Vector3.Transform(partVert, placementFrame));
            }
            return verts;
        }

        public void BuildBoundingBox()
        {
            var verts = GetVertices();
            BoundingBox = new BoundingBox(verts);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

namespace DatExplorer.Model
{
    public class Setup
    {
        public SetupModel _setup;

        public List<GfxObj> Parts;

        public List<Matrix> PlacementFrames;

        public BoundingBox BoundingBox;

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

            foreach (var placementFrame in _setup.PlacementFrames[0].AnimFrame.Frames)
                PlacementFrames.Add(placementFrame.ToXna());

            BuildBoundingBox();
        }

        public Setup(uint setupID, FileTypes.ObjDesc objDesc, Dictionary<int, uint> customPaletteColors)
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
                uint gfxObjID;
                GfxObj gfxObj;

                var apChange = objDesc.AnimPartChanges.Where(s => s.PartIndex == i).FirstOrDefault();
                if (apChange != null)
                {
                    gfxObjID = apChange.PartID;
                    gfxObj = new GfxObj(gfxObjID, false);
                    List<ACE.DatLoader.Entity.TextureMapChange> tmChanges = objDesc.TextureChanges.FindAll(s => s.PartIndex == i);


                    gfxObj.LoadTextures(tmChanges, customPaletteColors);
                    gfxObj.BuildPolygons();
                }
                else
                {
                    gfxObjID = _setup.Parts[i];
                    gfxObj = new GfxObj(gfxObjID, false);
                    gfxObj.LoadTextures(null, null);
                    gfxObj.BuildPolygons();
                }

                Parts.Add(gfxObj);
            }

            PlacementFrames = new List<Matrix>();

            foreach (var placementFrame in _setup.PlacementFrames[0].AnimFrame.Frames)
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
                var placementFrame = PlacementFrames[i];

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

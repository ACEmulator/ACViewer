using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class GfxObjInfo
    {
        public ACE.DatLoader.Entity.GfxObjInfo _gfxObjInfo;

        public GfxObjInfo(ACE.DatLoader.Entity.GfxObjInfo info)
        {
            _gfxObjInfo = info;
        }

        public List<TreeNode> BuildTree()
        {
            var id = new TreeNode($"ID: {_gfxObjInfo.Id:X8}", clickable: true);
            var degradeMode = new TreeNode($"DegradeMode: {_gfxObjInfo.DegradeMode}");
            var minDist = new TreeNode($"MinDist: {_gfxObjInfo.MinDist}");
            var idealDist = new TreeNode($"IdealDist: {_gfxObjInfo.IdealDist}");
            var maxDist = new TreeNode($"MaxDist; {_gfxObjInfo.MaxDist}");

            return new List<TreeNode>() { id, degradeMode, minDist, idealDist, maxDist };
        }
    }
}

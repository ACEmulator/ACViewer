using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class SkyObjectReplace
    {
        public ACE.DatLoader.Entity.SkyObjectReplace _skyObjReplace;

        public SkyObjectReplace(ACE.DatLoader.Entity.SkyObjectReplace skyObjReplace)
        {
            _skyObjReplace = skyObjReplace;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            var objIdx = new TreeNode($"ObjIdx: {_skyObjReplace.ObjectIndex}");
            treeNode.Add(objIdx);

            if (_skyObjReplace.GFXObjId != 0)
            {
                var gfxObjID = new TreeNode($"GfxObjID: {_skyObjReplace.GFXObjId:X8}");
                treeNode.Add(gfxObjID);
            }

            if (_skyObjReplace.Rotate != 0)
            {
                var rotate = new TreeNode($"Rotate: {_skyObjReplace.Rotate}");
                treeNode.Add(rotate);
            }

            if (_skyObjReplace.Transparent != 0)
            {
                var transparent = new TreeNode($"Transparent: {_skyObjReplace.Transparent}");
                treeNode.Add(transparent);
            }

            if (_skyObjReplace.Luminosity != 0)
            {
                var luminosity = new TreeNode($"Luminosity: {_skyObjReplace.Luminosity}");
                treeNode.Add(luminosity);
            }

            if (_skyObjReplace.MaxBright != 0)
            {
                var maxBright = new TreeNode($"MaxBrightness: {_skyObjReplace.MaxBright}");
                treeNode.Add(maxBright);
            }

            return treeNode;
        }
    }
}

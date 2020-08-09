using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class ObjectDesc
    {
        public ACE.DatLoader.Entity.ObjectDesc _desc;

        public ObjectDesc(ACE.DatLoader.Entity.ObjectDesc desc)
        {
            _desc = desc;
        }

        public List<TreeNode> BuildTree()
        {
            var objectID = new TreeNode($"Object ID: {_desc.ObjId:X8}");
            var baseLoc = new TreeNode($"BaseLoc: {new Frame(_desc.BaseLoc).ToString()}");
            var freq = new TreeNode($"Frequency: {_desc.Freq}");
            var displaceX = new TreeNode($"DisplaceX: {_desc.DisplaceX}");
            var displaceY = new TreeNode($"DisplaceY: {_desc.DisplaceY}");
            var minScale = new TreeNode($"MinScale: {_desc.MinScale}");
            var maxScale = new TreeNode($"MaxScale: {_desc.MaxScale}");
            var maxRotation = new TreeNode($"MaxRotation: {_desc.MaxRotation}");
            var minSlope = new TreeNode($"MinSlope: {_desc.MinSlope}");
            var maxSlope = new TreeNode($"MaxSlope: {_desc.MaxSlope}");

            var treeNode = new List<TreeNode>();
            treeNode.AddRange(new List<TreeNode>() { objectID, baseLoc, freq, displaceX, displaceY, minScale, maxScale, maxRotation, minSlope, maxSlope });

            if (_desc.Align != 0)
            {
                var align = new TreeNode($"Align: {_desc.Align}");
                treeNode.Add(align);
            }
            if (_desc.Orient != 0)
            {
                var orient = new TreeNode($"Orient: {_desc.Orient}");
                treeNode.Add(orient);
            }
            if (_desc.WeenieObj != 0)
            {
                var weenieObj = new TreeNode($"WeenieObj: {_desc.WeenieObj}");
                treeNode.Add(weenieObj);
            }

            return treeNode;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity.Enum;

namespace ACViewer.Entity
{
    public class BSPLeaf: BSPNode
    {
        public ACE.DatLoader.Entity.BSPLeaf _bspLeaf;

        public BSPLeaf(ACE.DatLoader.Entity.BSPLeaf bspLeaf)
        {
            _bspLeaf = bspLeaf;
        }

        public override List<TreeNode> BuildTree(BSPType bspType)
        {
            var type = new TreeNode($"Type: {_bspLeaf.Type}");
            var leafIdx = new TreeNode($"LeafIndex: {_bspLeaf.LeafIndex}");

            var treeNode = new List<TreeNode>() { type, leafIdx };

            if (bspType == BSPType.Physics)
            {
                var solid = new TreeNode($"Solid: {_bspLeaf.Solid}");
                var sphere = new TreeNode($"Sphere: {new Sphere(_bspLeaf.Sphere).ToString()}");
                var inPolys = new TreeNode($"InPolys: {String.Join(", ", _bspLeaf.InPolys)}");

                treeNode.AddRange(new List<TreeNode>() { solid, sphere, inPolys });
            }
            return treeNode;
        }
    }
}

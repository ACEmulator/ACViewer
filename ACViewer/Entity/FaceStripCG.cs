using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class FaceStripCG
    {
        public ACE.DatLoader.Entity.FaceStripCG _faceStrip;

        public FaceStripCG(ACE.DatLoader.Entity.FaceStripCG faceStrip)
        {
            _faceStrip = faceStrip;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_faceStrip.IconImage != 0)
            {
                var icon = new TreeNode($"Icon: {_faceStrip.IconImage:X8}");
                treeNode.Add(icon);
            }

            var objDesc = new TreeNode("ObjDesc:");
            objDesc.Items.AddRange(new ObjDesc(_faceStrip.ObjDesc).BuildTree());
            treeNode.Add(objDesc);

            return treeNode;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class EyeStripCG
    {
        public ACE.DatLoader.Entity.EyeStripCG _eyeStrip;

        public EyeStripCG(ACE.DatLoader.Entity.EyeStripCG eyeStrip)
        {
            _eyeStrip = eyeStrip;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_eyeStrip.IconImage != 0)
            {
                var iconImage = new TreeNode($"Icon: {_eyeStrip.IconImage:X8}");
                treeNode.Add(iconImage);
            }

            if (_eyeStrip.IconImageBald != 0)
            {
                var iconImageBald = new TreeNode($"Bald Icon: {_eyeStrip.IconImageBald:X8}");
                treeNode.Add(iconImageBald);
            }

            var objDesc = new TreeNode($"ObjDesc:");
            objDesc.Items.AddRange(new ObjDesc(_eyeStrip.ObjDesc).BuildTree());
            treeNode.Add(objDesc);

            var objDescBald = new TreeNode($"ObjDescBald:");
            objDescBald.Items.AddRange(new ObjDesc(_eyeStrip.ObjDescBald).BuildTree());
            treeNode.Add(objDescBald);

            return treeNode;
        }
    }
}

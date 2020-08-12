using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class HairStyleCG
    {
        public ACE.DatLoader.Entity.HairStyleCG _hairStyle;

        public HairStyleCG(ACE.DatLoader.Entity.HairStyleCG hairStyle)
        {
            _hairStyle = hairStyle;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();
            var icon = new TreeNode($"Icon: {_hairStyle.IconImage:X8}");
            treeNode.Add(icon);

            if (_hairStyle.Bald)
            {
                var bald = new TreeNode($"Bald: True");
                treeNode.Add(bald);
            }

            if (_hairStyle.AlternateSetup != 0)
            {
                var alternateSetup = new TreeNode($"Alternate Setup: {_hairStyle.AlternateSetup:X8}");
                treeNode.Add(alternateSetup);
            }
            var objDesc = new TreeNode("ObjDesc:");
            objDesc.Items.AddRange(new ObjDesc(_hairStyle.ObjDesc).BuildTree());
            treeNode.Add(objDesc);

            return treeNode;
        }
    }
}

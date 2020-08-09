using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class BldPortal
    {
        public ACE.DatLoader.Entity.CBldPortal _portal;

        public BldPortal(ACE.DatLoader.Entity.CBldPortal portal)
        {
            _portal = portal;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_portal.Flags != 0)
            {
                var flags = new TreeNode($"Flags: {_portal.Flags}");
                treeNode.Add(flags);
            }

            if (_portal.OtherCellId != 0)
            {
                var otherCellID = new TreeNode($"OtherCellID: {_portal.OtherCellId:X}");
                treeNode.Add(otherCellID);
            }

            if (_portal.OtherPortalId != 0)
            {
                var otherPortalID = new TreeNode($"OtherPortalID: {_portal.OtherPortalId:X}");
                treeNode.Add(otherPortalID);
            }

            return treeNode;
        }
    }
}

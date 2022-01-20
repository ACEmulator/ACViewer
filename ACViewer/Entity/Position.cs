using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class Position
    {
        public ACE.DatLoader.Entity.Position _position;

        public Position(ACE.DatLoader.Entity.Position position)
        {
            _position = position;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_position.ObjCellID != 0)
            {
                var objCellID = new TreeNode($"ObjCellID: {_position.ObjCellID:X8}", clickable: true);
                treeNode.Add(objCellID);
            }

            // frame inlined
            if (!_position.Frame.Origin.IsZeroEpsilon())
            {
                var origin = new TreeNode($"Origin: {_position.Frame.Origin}");
                treeNode.Add(origin);
            }

            if (!_position.Frame.Orientation.IsIdentity)
            {
                var orientation = new TreeNode($"Orientation: {_position.Frame.Orientation}");
                treeNode.Add(orientation);
            }
            return treeNode;
        }
    }
}

using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class AttackCone
    {
        public ACE.DatLoader.Entity.AttackCone _attackCone;

        public AttackCone(ACE.DatLoader.Entity.AttackCone attackCone)
        {
            _attackCone = attackCone;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            treeNode.Add(new TreeNode($"PartIndex: {_attackCone.PartIndex}"));
            
            treeNode.Add(new TreeNode($"LeftX: {_attackCone.LeftX}"));
            treeNode.Add(new TreeNode($"LeftY: {_attackCone.LeftY}"));

            treeNode.Add(new TreeNode($"RightX: {_attackCone.RightX}"));
            treeNode.Add(new TreeNode($"RightY: {_attackCone.RightY}"));

            treeNode.Add(new TreeNode($"Radius: {_attackCone.Radius}"));
            treeNode.Add(new TreeNode($"Height: {_attackCone.Height}"));

            return treeNode;
        }
    }
}

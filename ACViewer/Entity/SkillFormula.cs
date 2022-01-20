using System.Collections.Generic;

using ACE.Entity.Enum.Properties;

namespace ACViewer.Entity
{
    public class SkillFormula
    {
        public ACE.DatLoader.Entity.SkillFormula _skillFormula;

        public SkillFormula(ACE.DatLoader.Entity.SkillFormula skillFormula)
        {
            _skillFormula = skillFormula;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            treeNode.Add(new TreeNode($"Attr1: {(PropertyAttribute)_skillFormula.Attr1}"));
            treeNode.Add(new TreeNode($"Attr2: {(PropertyAttribute)_skillFormula.Attr2}"));
            treeNode.Add(new TreeNode($"W: {_skillFormula.W}"));
            treeNode.Add(new TreeNode($"X: {_skillFormula.X}"));
            treeNode.Add(new TreeNode($"Y: {_skillFormula.Y}"));
            treeNode.Add(new TreeNode($"Z (divisor): {_skillFormula.Z}"));

            return treeNode;
        }
    }
}

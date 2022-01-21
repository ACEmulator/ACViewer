using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class SkillBase
    {
        public enum SkillCategory
        {
            Undef  = 0,
            Combat = 1,
            Other  = 2,
            Magic  = 3
        };
        
        public ACE.DatLoader.Entity.SkillBase _skillBase;

        public SkillBase(ACE.DatLoader.Entity.SkillBase skillBase)
        {
            _skillBase = skillBase;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            treeNode.Add(new TreeNode($"Name: {_skillBase.Name}"));
            treeNode.Add(new TreeNode($"Description: {_skillBase.Description}"));
            treeNode.Add(new TreeNode($"Icon: {_skillBase.IconId:X8}", clickable: true));
            treeNode.Add(new TreeNode($"TrainedCost: {_skillBase.TrainedCost}"));
            treeNode.Add(new TreeNode($"SpecializedCost: {_skillBase.SpecializedCost}"));
            treeNode.Add(new TreeNode($"Category: {(SkillCategory)_skillBase.Category}"));
            treeNode.Add(new TreeNode($"CharGenUse: {_skillBase.ChargenUse}"));
            treeNode.Add(new TreeNode($"MinLevel: {_skillBase.MinLevel}"));

            var skillFormula = new TreeNode($"SkillFormula");
            skillFormula.Items = new SkillFormula(_skillBase.Formula).BuildTree();
            treeNode.Add(skillFormula);

            treeNode.Add(new TreeNode($"UpperBound: {_skillBase.UpperBound}"));
            treeNode.Add(new TreeNode($"LowerBound: {_skillBase.LowerBound}"));
            treeNode.Add(new TreeNode($"LearnMod: {_skillBase.LearnMod}"));

            return treeNode;
        }
    }
}

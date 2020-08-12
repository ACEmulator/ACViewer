using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity.Enum;

namespace ACViewer.Entity
{
    public class SkillCG
    {
        public ACE.DatLoader.Entity.SkillCG _skill;

        public SkillCG(ACE.DatLoader.Entity.SkillCG skill)
        {
            _skill = skill;
        }

        public List<TreeNode> BuildTree()
        {
            var skill = new TreeNode($"Skill: {(Skill)_skill.SkillNum}");
            var normalCost = new TreeNode($"Normal Cost: {_skill.NormalCost}");
            var primaryCost = new TreeNode($"Primary Cost: {_skill.PrimaryCost}");

            return new List<TreeNode>() { skill, normalCost, primaryCost };
        }
    }
}

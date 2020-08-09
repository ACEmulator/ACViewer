using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class CombatManeuver
    {
        public ACE.DatLoader.Entity.CombatManeuver _combatManeuver;

        public CombatManeuver(ACE.DatLoader.Entity.CombatManeuver combatManeuver)
        {
            _combatManeuver = combatManeuver;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            var style = new TreeNode($"Stance: {_combatManeuver.Style}");
            var attackHeight = new TreeNode($"Attack Height: {_combatManeuver.AttackHeight}");
            var attackType = new TreeNode($"Attack Type: {_combatManeuver.AttackType}");

            treeNode.AddRange(new List<TreeNode>() { style, attackHeight, attackType });

            if (_combatManeuver.MinSkillLevel != 0)
            {
                var minSkillLevel = new TreeNode($"Min Skill: {_combatManeuver.MinSkillLevel}");
                treeNode.Add(minSkillLevel);
            }
            var motion = new TreeNode($"Motion: {_combatManeuver.Motion}");
            treeNode.Add(motion);

            return treeNode;
        }
    }
}

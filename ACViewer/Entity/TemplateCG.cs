using System.Collections.Generic;

using ACE.Entity.Enum;

namespace ACViewer.Entity
{
    public class TemplateCG
    {
        public ACE.DatLoader.Entity.TemplateCG _template;

        public TemplateCG(ACE.DatLoader.Entity.TemplateCG template)
        {
            _template = template;
        }

        public List<TreeNode> BuildTree()
        {
            var name = new TreeNode($"Name: {_template.Name}");
            var icon = new TreeNode($"Icon: {_template.IconImage:X8}", clickable: true);
            var title = new TreeNode($"Title: {(CharacterTitle)_template.Title}");

            var strength = new TreeNode($"Strength: {_template.Strength}");
            var endurance = new TreeNode($"Endurnace: {_template.Endurance}");
            var coordination = new TreeNode($"Coordination: {_template.Coordination}");
            var quickness = new TreeNode($"Quickness: {_template.Quickness}");
            var focus = new TreeNode($"Focus: {_template.Focus}");
            var self = new TreeNode($"Self: {_template.Self}");

            var treeNode = new List<TreeNode>() { name, icon, title, strength, endurance, coordination, quickness, focus, self };

            if (_template.NormalSkillsList.Count > 0)
            {
                var normalSkills = new TreeNode("Normal Skills:");
                foreach (var skillID in _template.NormalSkillsList)
                    normalSkills.Items.Add(new TreeNode($"{(Skill)skillID}"));

                treeNode.Add(normalSkills);
            }

            if (_template.PrimarySkillsList.Count > 0)
            {
                var primarySkills = new TreeNode("Primary Skills:");
                foreach (var skillID in _template.PrimarySkillsList)
                    primarySkills.Items.Add(new TreeNode($"{(Skill)skillID}"));

                treeNode.Add(primarySkills);
            }
            return treeNode;
        }
    }
}

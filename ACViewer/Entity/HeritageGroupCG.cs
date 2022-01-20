using System.Collections.Generic;

using ACE.Entity.Enum;

namespace ACViewer.Entity
{
    public class HeritageGroupCG
    {
        public ACE.DatLoader.Entity.HeritageGroupCG _hg;

        public HeritageGroupCG(ACE.DatLoader.Entity.HeritageGroupCG hg)
        {
            _hg = hg;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            var name = new TreeNode($"Name: {_hg.Name}");
            var icon = new TreeNode($"Icon: {_hg.IconImage:X8}", clickable: true);
            var setup = new TreeNode($"Setup: {_hg.SetupID:X8}", clickable: true);
            var environment = new TreeNode($"Environment: {_hg.EnvironmentSetupID:X8}", clickable: true);
            var attribCredits = new TreeNode($"Attribute Credits: {_hg.AttributeCredits}");
            var skillCredits = new TreeNode($"Skill Credits: {_hg.SkillCredits}");
            var primaryStartAreas = new TreeNode($"Primary Start Areas: {string.Join(",", _hg.PrimaryStartAreas)}");
            var secondaryStartAreas = new TreeNode($"Secondary Start Areas: {string.Join(",", _hg.SecondaryStartAreas)}");

            var skills = new TreeNode("Skills:");
            foreach (var skill in _hg.Skills)
            {
                var skillTree = new SkillCG(skill).BuildTree();
                var skillNode = new TreeNode(skillTree[0].Name.Replace("Skill: ", ""));
                skillNode.Items.AddRange(new List<TreeNode>() { skillTree[1], skillTree[2] });
                skills.Items.Add(skillNode);
            }

            var templates = new TreeNode("Templates:");
            foreach (var template in _hg.Templates)
            {
                var templateTree = new TemplateCG(template).BuildTree();
                var templateNode = new TreeNode(templateTree[0].Name.Replace("Name: ", ""));
                templateTree.RemoveAt(0);
                templateNode.Items.AddRange(templateTree);
                templates.Items.Add(templateNode);
            }

            var genders = new TreeNode("Genders:");
            foreach (var kvp in _hg.Genders)
            {
                var gender = new TreeNode($"{(Gender)kvp.Key}");
                var genderTree = new SexCG(kvp.Value).BuildTree();
                genderTree.RemoveAt(0);
                gender.Items.AddRange(genderTree);
                genders.Items.Add(gender);
            }

            treeNode.AddRange(new List<TreeNode>() { name, icon, setup, environment, attribCredits, skillCredits, primaryStartAreas, secondaryStartAreas, skills, templates, genders });

            return treeNode;
        }
    }
}

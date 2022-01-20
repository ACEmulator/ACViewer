using System.Linq;

using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class SkillTable
    {
        public ACE.DatLoader.FileTypes.SkillTable _skillTable;

        public SkillTable(ACE.DatLoader.FileTypes.SkillTable skillTable)
        {
            _skillTable = skillTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_skillTable.Id:X8}");

            foreach (var skill in _skillTable.SkillBaseHash.OrderBy(i => i.Key))
            {
                // skip retired skills, empty data
                if (string.IsNullOrEmpty(skill.Value.Name))
                    continue;
                
                var skillNode = new TreeNode($"{skill.Key}: {skill.Value.Name}");
                skillNode.Items = new SkillBase(skill.Value).BuildTree();

                treeView.Items.Add(skillNode);
            }
            return treeView;
        }
    }
}

using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class SecondaryAttributeTable
    {
        public ACE.DatLoader.FileTypes.SecondaryAttributeTable _vitalTable;

        public SecondaryAttributeTable(ACE.DatLoader.FileTypes.SecondaryAttributeTable vitalTable)
        {
            _vitalTable = vitalTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_vitalTable.Id:X8}");

            var health = new TreeNode($"Health");
            health.Items =  new SkillFormula(_vitalTable.MaxHealth.Formula).BuildTree();
            treeView.Items.Add(health);

            var stamina = new TreeNode($"Stamina");
            stamina.Items = new SkillFormula(_vitalTable.MaxStamina.Formula).BuildTree();
            treeView.Items.Add(stamina);

            var mana = new TreeNode($"Mana");
            mana.Items = new SkillFormula(_vitalTable.MaxMana.Formula).BuildTree();
            treeView.Items.Add(mana);

            return treeView;
        }
    }
}

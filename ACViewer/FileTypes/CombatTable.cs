using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class CombatTable
    {
        public ACE.DatLoader.FileTypes.CombatManeuverTable _combatTable;

        public CombatTable(ACE.DatLoader.FileTypes.CombatManeuverTable combatTable)
        {
            _combatTable = combatTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_combatTable.Id:X8}");

            var maneuvers = new TreeNode("Maneuvers:");

            for (var i = 0; i < _combatTable.CMT.Count; i++)
            {
                var maneuver = new TreeNode($"{i}");
                maneuver.Items.AddRange(new CombatManeuver(_combatTable.CMT[i]).BuildTree());

                maneuvers.Items.Add(maneuver);
            }
            treeView.Items.AddRange(maneuvers.Items);

            return treeView;
        }
    }
}

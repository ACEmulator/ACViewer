using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.DatLoader.Entity;
using ACE.Entity.Enum;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class SpellTable
    {
        public ACE.DatLoader.FileTypes.SpellTable _spellTable;

        public SpellTable(ACE.DatLoader.FileTypes.SpellTable spellTable)
        {
            _spellTable = spellTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_spellTable.Id:X8}");

            var spells = new TreeNode("Spells");
            
            foreach (var kvp in _spellTable.Spells)
            {
                var spellNode = new TreeNode($"{kvp.Key}: {kvp.Value.Name}");
                spellNode.Items = new Entity.SpellBase(kvp.Value).BuildTree();
                spells.Items.Add(spellNode);
            }

            var spellSets = new TreeNode($"Spell Sets");
            
            foreach (var kvp in _spellTable.SpellSet.OrderBy(i => i.Key))
            {
                var spellSetNode = new TreeNode($"{kvp.Key}: {(EquipmentSet)kvp.Key}");
                spellSetNode.Items = new Entity.SpellSet(kvp.Value).BuildTree();
                spellSets.Items.Add(spellSetNode);
            }

            treeView.Items = new List<TreeNode>() { spells, spellSets };
            
            return treeView;
        }
    }
}

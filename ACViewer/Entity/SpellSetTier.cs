using System.Collections.Generic;

using ACE.DatLoader;

namespace ACViewer.Entity
{
    public class SpellSetTier
    {
        public ACE.DatLoader.FileTypes.SpellSetTiers _spellSetTier;

        public SpellSetTier(ACE.DatLoader.FileTypes.SpellSetTiers spellSetTier)
        {
            _spellSetTier = spellSetTier;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            var spellTable = DatManager.PortalDat.SpellTable;

            foreach (var spellId in _spellSetTier.Spells)
            {
                var spell = spellTable.Spells[spellId];
                treeNode.Add(new TreeNode($"{spellId} - {spell.Name}"));
            }
            return treeNode;
        }
    }
}

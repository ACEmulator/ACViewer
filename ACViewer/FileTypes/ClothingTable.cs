using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class ClothingTable
    {
        public ACE.DatLoader.FileTypes.ClothingTable _clothingTable;

        public ClothingTable(ACE.DatLoader.FileTypes.ClothingTable clothingTable)
        {
            _clothingTable = clothingTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_clothingTable.Id:X8}");

            var baseEffects = new TreeNode("Base Effects:");
            foreach (var kvp in _clothingTable.ClothingBaseEffects)
            {
                var baseEffect = new TreeNode($"{kvp.Key:X8}");
                baseEffect.Items.AddRange(new ClothingBaseEffect(kvp.Value).BuildTree());

                baseEffects.Items.Add(baseEffect);
            }

            var subPaletteEffects = new TreeNode("SubPalette Effects:");
            foreach (var kvp in _clothingTable.ClothingSubPalEffects)
            {
                var subPaletteEffect = new TreeNode($"{kvp.Key:D2}");
                subPaletteEffect.Items.AddRange(new ClothingSubPaletteEffect(kvp.Value).BuildTree());

                subPaletteEffects.Items.Add(subPaletteEffect);
            }

            treeView.Items.AddRange(new List<TreeNode>() { baseEffects, subPaletteEffects });

            return treeView;
        }
    }
}

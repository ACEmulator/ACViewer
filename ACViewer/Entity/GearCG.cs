using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class GearCG
    {
        public ACE.DatLoader.Entity.GearCG _gear;

        public GearCG(ACE.DatLoader.Entity.GearCG gear)
        {
            _gear = gear;
        }

        public List<TreeNode> BuildTree()
        {
            var name = new TreeNode($"Name: {_gear.Name}");
            var clothingTable = new TreeNode($"Clothing Table: {_gear.ClothingTable:X8}");
            var weenieDefault = new TreeNode($"Weenie Default: {_gear.WeenieDefault:X8}");

            return new List<TreeNode>() { name, clothingTable, weenieDefault };
        }
    }
}

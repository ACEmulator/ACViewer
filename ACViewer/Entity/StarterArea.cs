using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class StarterArea
    {
        public ACE.DatLoader.Entity.StarterArea _starterArea;

        public StarterArea(ACE.DatLoader.Entity.StarterArea starterArea)
        {
            _starterArea = starterArea;
        }

        public List<TreeNode> BuildTree()
        {
            var name = new TreeNode($"Name: {_starterArea.Name}");
            var locations = new TreeNode("Locations:");
            for (var i = 0; i < _starterArea.Locations.Count; i++)
            {
                var position = new Position(_starterArea.Locations[i]).BuildTree();
                var location = new TreeNode($"{position[0].Name.Replace("ObjCellID: ", "")}");
                location.Items.AddRange(new List<TreeNode>() { position[1], position[2] });
                locations.Items.Add(location);
            }
            return new List<TreeNode>() { name, locations };
        }
    }
}

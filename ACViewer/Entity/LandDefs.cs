using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class LandDefs
    {
        public ACE.DatLoader.Entity.LandDefs _landDefs;

        public LandDefs(ACE.DatLoader.Entity.LandDefs landDefs)
        {
            _landDefs = landDefs;
        }

        public List<TreeNode> BuildTree()
        {
            var numBlockLength = new TreeNode($"NumBlockLength: {_landDefs.NumBlockLength}");
            var numBlockWidth = new TreeNode($"NumBlockWidth: {_landDefs.NumBlockWidth}");
            var squareLength = new TreeNode($"SquareLength: {_landDefs.SquareLength}");
            var lBlockLength = new TreeNode($"LBlockLength: {_landDefs.LBlockLength}");
            var vertexPerCell = new TreeNode($"VertexPerCell: {_landDefs.VertexPerCell}");
            var maxObjHeight = new TreeNode($"MaxObjHeight: {_landDefs.MaxObjHeight}");
            var skyHeight = new TreeNode($"SkyHeight: {_landDefs.SkyHeight}");
            var roadWidth = new TreeNode($"RoadWidth: {_landDefs.RoadWidth}");

            var landHeightTable = new TreeNode("LandHeightTable:");
            for (var i = 0; i < _landDefs.LandHeightTable.Count; i++)
                landHeightTable.Items.Add(new TreeNode($"{i}: {_landDefs.LandHeightTable[i]}"));

            return new List<TreeNode>() { numBlockLength, numBlockWidth, squareLength, lBlockLength, vertexPerCell, maxObjHeight, skyHeight, roadWidth, landHeightTable };
        }
    }
}

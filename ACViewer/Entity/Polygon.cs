using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class Polygon
    {
        public ACE.DatLoader.Entity.Polygon _polygon;

        public Polygon(ACE.DatLoader.Entity.Polygon polygon)
        {
            _polygon = polygon;
        }

        public List<TreeNode> BuildTree()
        {
            //var numPoints = new TreeNode($"NumPoints: {_polygon.NumPts}");
            var stippling = new TreeNode($"Stippling: {_polygon.Stippling}");
            var cullMode = new TreeNode($"CullMode: {_polygon.SidesType}");
            var posSurface = new TreeNode($"PosSurface: {_polygon.PosSurface}");
            var negSurface = new TreeNode($"NegSurface: {_polygon.NegSurface}");
            var vertexIDs = new TreeNode($"Vertex IDs: {string.Join(", ", _polygon.VertexIds)}");
            var posUVIndices = new TreeNode($"PosUVIndices: {string.Join(", ", _polygon.PosUVIndices)}");
            var negUVIndices = new TreeNode($"NegUVIndices: {string.Join(", ", _polygon.NegUVIndices)}");

            return new List<TreeNode>() { stippling, cullMode, posSurface, negSurface, vertexIDs, posUVIndices, negUVIndices };
        }
    }
}

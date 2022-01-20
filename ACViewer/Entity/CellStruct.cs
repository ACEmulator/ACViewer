using System.Collections.Generic;

using ACE.Entity.Enum;

namespace ACViewer.Entity
{
    public class CellStruct
    {
        public ACE.DatLoader.Entity.CellStruct _cellStruct;

        public CellStruct(ACE.DatLoader.Entity.CellStruct cellStruct)
        {
            _cellStruct = cellStruct;
        }

        public List<TreeNode> BuildTree()
        {
            var vertexArray = new TreeNode("VertexArray:");
            vertexArray.Items.AddRange(new VertexArray(_cellStruct.VertexArray).BuildTree());

            var polygons = new TreeNode("Polygons:");
            foreach (var kvp in _cellStruct.Polygons)
            {
                var polygon = new TreeNode($"{kvp.Key}");
                polygon.Items.AddRange(new Polygon(kvp.Value).BuildTree());

                polygons.Items.Add(polygon);
            }

            var portals = new TreeNode("Portals:");
            foreach (var portalID in _cellStruct.Portals)
                portals.Items.Add(new TreeNode($"{portalID:X8}"));

            var cellBSP = new TreeNode("CellBSP:");
            cellBSP.Items.AddRange(new BSPTree(_cellStruct.CellBSP).BuildTree(BSPType.Cell).Items);

            var physicsPolygons = new TreeNode("PhysicsPolygons:");
            foreach (var kvp in _cellStruct.PhysicsPolygons)
            {
                var physicsPolygon = new TreeNode($"{kvp.Key}");
                physicsPolygon.Items.AddRange(new Polygon(kvp.Value).BuildTree());

                physicsPolygons.Items.Add(physicsPolygon);
            }

            var physicsBSP = new TreeNode("PhysicsBSP:");
            physicsBSP.Items.AddRange(new BSPTree(_cellStruct.PhysicsBSP).BuildTree(BSPType.Physics).Items);

            var treeNode = new List<TreeNode>() { vertexArray, polygons, portals, cellBSP, physicsPolygons, physicsBSP };

            if (_cellStruct.DrawingBSP != null)
            {
                var drawingBSP = new TreeNode("DrawingBSP:");
                drawingBSP.Items.AddRange(new BSPTree(_cellStruct.DrawingBSP).BuildTree(BSPType.Drawing).Items);

                treeNode.Add(drawingBSP);
            }

            return treeNode;
        }
    }
}

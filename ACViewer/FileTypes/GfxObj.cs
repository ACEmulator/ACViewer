using System;
using System.Collections.Generic;
using System.Linq;

using ACE.Entity.Enum;

using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class GfxObj
    {
        public ACE.DatLoader.FileTypes.GfxObj _gfxObj;

        public GfxObj(ACE.DatLoader.FileTypes.GfxObj gfxObj)
        {
            _gfxObj = gfxObj;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_gfxObj.Id:X8}");

            var surfaces = new TreeNode("Surfaces");

            for (var i = 0; i < _gfxObj.Surfaces.Count; i++)
            {
                var surface = _gfxObj.Surfaces[i];
                surfaces.Items.Add(new TreeNode($"Surface {i}: {surface:X8}", clickable: true));
            }

            // Build surface usage summary
            var surfaceUsageSummary = BuildSurfaceUsageSummary();

            var vertexArray = new TreeNode("VertexArray");
            foreach (var item in new VertexArray(_gfxObj.VertexArray).BuildTree())
                vertexArray.Items.Add(item);

            var nodesToAdd = new List<TreeNode>() { surfaces };
            if (surfaceUsageSummary != null)
                nodesToAdd.Add(surfaceUsageSummary);
            nodesToAdd.Add(vertexArray);

            treeView.Items.AddRange(nodesToAdd);

            if (_gfxObj.Flags.HasFlag(GfxObjFlags.HasPhysics))
            {
                var physicsPolygons = new TreeNode("PhysicsPolygons");
                foreach (var kvp in _gfxObj.PhysicsPolygons)
                {
                    var physicsPolygon = new TreeNode($"{kvp.Key}");
                    physicsPolygon.Items.AddRange(new Polygon(kvp.Value).BuildTree());
                    physicsPolygons.Items.Add(physicsPolygon);
                }

                var physicsBSP = new TreeNode("PhysicsBSP");
                if (_gfxObj.PhysicsBSP != null)
                    physicsBSP.Items.AddRange(new BSPTree(_gfxObj.PhysicsBSP).BuildTree(BSPType.Physics).Items);

                treeView.Items.AddRange(new List<TreeNode>() { physicsPolygons, physicsBSP });
            }

            var sortCenter = new TreeNode($"SortCenter: {_gfxObj.SortCenter}");
            treeView.Items.Add(sortCenter);

            if (_gfxObj.Flags.HasFlag(GfxObjFlags.HasDrawing))
            {
                var polygons = new TreeNode("Polygons");
                foreach (var kvp in _gfxObj.Polygons)
                {
                    var polygon = new TreeNode($"{kvp.Key}");
                    polygon.Items.AddRange(new Polygon(kvp.Value).BuildTree());
                    polygons.Items.Add(polygon);
                }

                var drawingBSP = new TreeNode("DrawingBSP");
                if (_gfxObj.DrawingBSP != null)
                    drawingBSP.Items.AddRange(new BSPTree(_gfxObj.DrawingBSP).BuildTree(BSPType.Drawing).Items);

                treeView.Items.AddRange(new List<TreeNode>() { polygons, drawingBSP });
            }

            if (_gfxObj.Flags.HasFlag(GfxObjFlags.HasDIDDegrade))
            {
                var didDegrade = new TreeNode($"DIDDegrade: {_gfxObj.DIDDegrade:X8}", clickable: true);
                treeView.Items.Add(didDegrade);
            }

            return treeView;
        }

        private TreeNode BuildSurfaceUsageSummary()
        {
            if (_gfxObj.Polygons == null || _gfxObj.Polygons.Count == 0)
                return null;

            // Map surface index -> list of polygon indices that use it
            var surfaceToPolygons = new Dictionary<int, List<int>>();

            foreach (var kvp in _gfxObj.Polygons)
            {
                var polygonIndex = kvp.Key;
                var polygon = kvp.Value;

                // Track PosSurface
                if (polygon.PosSurface >= 0 && polygon.PosSurface < _gfxObj.Surfaces.Count)
                {
                    if (!surfaceToPolygons.ContainsKey((int)polygon.PosSurface))
                        surfaceToPolygons[(int)polygon.PosSurface] = new List<int>();
                    surfaceToPolygons[(int)polygon.PosSurface].Add(polygonIndex);
                }

                // Track NegSurface if different from PosSurface
                if (polygon.NegSurface >= 0 && polygon.NegSurface < _gfxObj.Surfaces.Count && polygon.NegSurface != polygon.PosSurface)
                {
                    if (!surfaceToPolygons.ContainsKey((int)polygon.NegSurface))
                        surfaceToPolygons[(int)polygon.NegSurface] = new List<int>();
                    surfaceToPolygons[(int)polygon.NegSurface].Add(polygonIndex);
                }
            }

            if (surfaceToPolygons.Count == 0)
                return null;

            var summaryNode = new TreeNode("Surface Usage Summary:");

            foreach (var surfaceIdx in surfaceToPolygons.Keys.OrderBy(k => k))
            {
                var polygonIndices = surfaceToPolygons[surfaceIdx];
                var surfaceId = _gfxObj.Surfaces[surfaceIdx];

                var usageNode = new TreeNode($"Surface {surfaceIdx} ({surfaceId:X8}): Used by {polygonIndices.Count} polygon(s)");

                // Show first few polygon indices
                var polygonList = string.Join(", ", polygonIndices.Take(10));
                if (polygonIndices.Count > 10)
                    polygonList += $", ... ({polygonIndices.Count - 10} more)";

                usageNode.Items.Add(new TreeNode($"Polygons: {polygonList}"));

                summaryNode.Items.Add(usageNode);
            }

            return summaryNode;
        }
    }
}

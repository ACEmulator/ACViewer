using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class Vertex
    {
        public ACE.DatLoader.Entity.SWVertex _swVertex;

        public Vertex(ACE.DatLoader.Entity.SWVertex swVertex)
        {
            _swVertex = swVertex;
        }

        public List<TreeNode> BuildTree()
        {
            var origin = new TreeNode($"Origin: {_swVertex.Origin}");
            var normal = new TreeNode($"Normal: {_swVertex.Normal}");
            var uvs = new TreeNode("UVs");

            foreach (var uv in _swVertex.UVs)
                uvs.Items.Add(new UV(uv).BuildTree());

            return new List<TreeNode>() { origin, normal, uvs };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ACViewer.Entity
{
    public class VertexArray
    {
        public ACE.DatLoader.Entity.CVertexArray _vertexArray;

        public VertexArray(ACE.DatLoader.Entity.CVertexArray vertexArray)
        {
            _vertexArray = vertexArray;
        }

        public List<TreeNode> BuildTree()
        {
            var vertexType = new TreeNode($"VertexType: {_vertexArray.VertexType}");

            var vertices = new TreeNode("Vertices");

            foreach (var kvp in _vertexArray.Vertices)
            {
                var vertex = new TreeNode($"{kvp.Key}");

                foreach (var item in new Vertex(kvp.Value).BuildTree())
                    vertex.Items.Add(item);

                vertices.Items.Add(vertex);
            }
            return new List<TreeNode>() { vertexType, vertices };
        }
    }
}

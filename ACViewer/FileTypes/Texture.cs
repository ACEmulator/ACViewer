using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class Texture
    {
        public ACE.DatLoader.FileTypes.Texture _texture;

        public Texture(ACE.DatLoader.FileTypes.Texture texture)
        {
            _texture = texture;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_texture.Id:X8}");

            var unknown = new TreeNode($"Unknown: {_texture.Unknown}");
            var width = new TreeNode($"Width: {_texture.Width}");
            var height = new TreeNode($"Height: {_texture.Height}");
            var format = new TreeNode($"Type: {_texture.Format}");
            var size = new TreeNode($"Size: {_texture.Length} bytes");

            treeView.Items.AddRange(new List<TreeNode>() { unknown, width, height, format, size });

            return treeView;
        }
    }
}

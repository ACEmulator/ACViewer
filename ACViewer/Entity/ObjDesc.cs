using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class ObjDesc
    {
        public ACE.DatLoader.Entity.ObjDesc _objDesc;

        public ObjDesc(ACE.DatLoader.Entity.ObjDesc objDesc)
        {
            _objDesc = objDesc;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_objDesc.PaletteID != 0)
            {
                var paletteID = new TreeNode($"Palette ID: {_objDesc.PaletteID:X8}");
                treeNode.Add(paletteID);
            }

            if (_objDesc.SubPalettes.Count > 0)
            {
                var subPalettes = new TreeNode("SubPalettes:");
                foreach (var subPalette in _objDesc.SubPalettes)
                {
                    var subPaletteTree = new SubPalette(subPalette).BuildTree();
                    var subPaletteNode = new TreeNode(subPaletteTree[0].Name);
                    subPaletteTree.RemoveAt(0);
                    subPaletteNode.Items.AddRange(subPaletteTree);
                    subPalettes.Items.Add(subPaletteNode);
                }
                treeNode.Add(subPalettes);
            }

            if (_objDesc.TextureChanges.Count > 0)
            {
                var textureChanges = new TreeNode("Texture Changes:");
                foreach (var textureMapChange in _objDesc.TextureChanges)
                    textureChanges.Items.Add(new TreeNode(new TextureMapChange(textureMapChange).ToString()));

                treeNode.Add(textureChanges);
            }

            if (_objDesc.AnimPartChanges.Count > 0)
            {
                var animChanges = new TreeNode("AnimPart Changes:");
                foreach (var animChange in _objDesc.AnimPartChanges)
                    animChanges.Items.Add(new TreeNode(new AnimPartChange(animChange).ToString()));

                treeNode.Add(animChanges);
            }

            return treeNode;
        }
    }
}

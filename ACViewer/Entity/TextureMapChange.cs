using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class TextureMapChange
    {
        public ACE.DatLoader.Entity.TextureMapChange _textureChange;

        public TextureMapChange(ACE.DatLoader.Entity.TextureMapChange textureChange)
        {
            _textureChange = textureChange;
        }

        public List<TreeNode> BuildTree()
        {
            var partIdx = new TreeNode($"PartIdx: {_textureChange.PartIndex}");
            var oldTexture = new TreeNode($"Old Texture: {_textureChange.OldTexture:X8}", clickable: true);
            var newTexture = new TreeNode($"New Texture: {_textureChange.NewTexture:X8}", clickable: true);

            return new List<TreeNode>() { partIdx, oldTexture, newTexture };
        }

        public override string ToString()
        {
            return $"PartIdx: {_textureChange.PartIndex}, Old Tex: {_textureChange.OldTexture:X8}, New Tex: {_textureChange.NewTexture:X8}";
        }
    }
}

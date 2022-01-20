using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class ClothingObjectEffect
    {
        public ACE.DatLoader.Entity.CloObjectEffect _effect;

        public ClothingObjectEffect(ACE.DatLoader.Entity.CloObjectEffect effect)
        {
            _effect = effect;
        }

        public List<TreeNode> BuildTree()
        {
            var idx = new TreeNode($"PartIdx: {_effect.Index}");
            var modelID = new TreeNode($"Model ID: {_effect.ModelId:X8}", clickable: true);

            var textureEffects = new TreeNode("Texture Effects:");
            foreach (var textureEffect in _effect.CloTextureEffects)
                textureEffects.Items.Add(new TreeNode(new ClothingTextureEffect(textureEffect).ToString(), clickable: true));

            return new List<TreeNode>() { idx, modelID, textureEffects };
        }
    }
}

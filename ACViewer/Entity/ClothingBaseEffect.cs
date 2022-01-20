using System.Collections.Generic;
using System.Linq;

namespace ACViewer.Entity
{
    public class ClothingBaseEffect
    {
        public ACE.DatLoader.Entity.ClothingBaseEffect _baseEffect;

        public ClothingBaseEffect(ACE.DatLoader.Entity.ClothingBaseEffect baseEffect)
        {
            _baseEffect = baseEffect;
        }

        public List<TreeNode> BuildTree()
        {
            var objEffects = new TreeNode($"Object Effects:");
            foreach (var objEffect in _baseEffect.CloObjectEffects.OrderBy(i => i.Index))
            {
                var objEffectTree = new ClothingObjectEffect(objEffect).BuildTree();
                var objEffectNode = new TreeNode($"{objEffectTree[0].Name}");
                objEffectTree.RemoveAt(0);
                objEffectNode.Items.AddRange(objEffectTree);
                objEffects.Items.Add(objEffectNode);
            }
            return new List<TreeNode>() { objEffects };
        }
    }
}

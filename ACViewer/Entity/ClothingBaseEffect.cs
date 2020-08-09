using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            foreach (var objEffect in _baseEffect.CloObjectEffects)
            {
                var objEffectTree = new ClothingObjectEffect(objEffect).BuildTree();
                var objEffectNode = new TreeNode($"{objEffectTree[0].Name.Replace("Idx: ", "")}");
                objEffectTree.RemoveAt(0);
                objEffectNode.Items.AddRange(objEffectTree);
                objEffects.Items.Add(objEffectNode);
            }
            return new List<TreeNode>() { objEffects };
        }
    }
}

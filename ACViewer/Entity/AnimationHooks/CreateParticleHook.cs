using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class CreateParticleHook: AnimationHook
    {
        public CreateParticleHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.CreateParticleHook _createParticleHook)
            {
                treeNode.Add(new TreeNode($"EmitterInfoId: {_createParticleHook.EmitterInfoId:X8}"));
                treeNode.Add(new TreeNode($"PartIndex: {(int)_createParticleHook.PartIndex}"));
                treeNode.Add(new TreeNode($"Offset: {new Frame(_createParticleHook.Offset)}"));
                treeNode.Add(new TreeNode($"EmitterId: {_createParticleHook.EmitterId}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}

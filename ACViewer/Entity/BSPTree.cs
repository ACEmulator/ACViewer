using ACE.Entity.Enum;

namespace ACViewer.Entity
{
    public class BSPTree
    {
        public ACE.DatLoader.Entity.BSPTree _bspTree;

        public BSPTree(ACE.DatLoader.Entity.BSPTree bspTree)
        {
            _bspTree = bspTree;
        }

        public TreeNode BuildTree(BSPType bspType)
        {
            var root = new TreeNode("Root");
            root.Items.AddRange(new BSPNode(_bspTree.RootNode).BuildTree(bspType));

            return root;
        }
    }
}

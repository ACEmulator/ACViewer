using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class LandSurf
    {
        public ACE.DatLoader.Entity.LandSurf _landSurf;

        public LandSurf(ACE.DatLoader.Entity.LandSurf landSurf)
        {
            _landSurf = landSurf;
        }

        public List<TreeNode> BuildTree()
        {
            // type always 0?
            return new TexMerge(_landSurf.TexMerge).BuildTree();
        }
    }
}

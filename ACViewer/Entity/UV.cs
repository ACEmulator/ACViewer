namespace ACViewer.Entity
{
    public class UV
    {
        public ACE.DatLoader.Entity.Vec2Duv _uv;

        public UV(ACE.DatLoader.Entity.Vec2Duv uv)
        {
            _uv = uv;
        }

        public TreeNode BuildTree()
        {
            return new TreeNode($"U: {_uv.U} V: {_uv.V}");
        }
    }
}

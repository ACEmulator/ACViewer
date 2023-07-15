using Assimp;

namespace ACViewer.Model
{
    public class MaterialIdx
    {
        public int MaterialId { get; set; }
        public Material Material { get; set; }

        public MaterialIdx(int materialId, Material material = null)
        {
            MaterialId = materialId;
            Material = material;
        }
    }
}

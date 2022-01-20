using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class Palette
    {
        public ACE.DatLoader.FileTypes.Palette _palette;

        public Palette(ACE.DatLoader.FileTypes.Palette palette)
        {
            _palette = palette;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_palette.Id:X8}");

            foreach (var color in _palette.Colors)
                treeView.Items.Add(new TreeNode(Color.ToRGBA(color)));

            return treeView;
        }
    }
}

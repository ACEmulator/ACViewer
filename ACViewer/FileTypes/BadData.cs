using System.Linq;

using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class BadData
    {
        public ACE.DatLoader.FileTypes.BadData _badData;

        public BadData(ACE.DatLoader.FileTypes.BadData badData)
        {
            _badData = badData;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_badData.Id:X8}");

            var wcids = _badData.Bad.Keys.OrderBy(i => i).ToList();

            foreach (var wcid in wcids)
                treeView.Items.Add(new TreeNode($"{wcid}"));

            return treeView;
        }
    }
}

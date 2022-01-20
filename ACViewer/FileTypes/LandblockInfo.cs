using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class LandblockInfo
    {
        public ACE.DatLoader.FileTypes.LandblockInfo _info;

        public LandblockInfo(ACE.DatLoader.FileTypes.LandblockInfo info)
        {
            _info = info;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_info.Id:X8}");

            var numCells = new TreeNode($"NumCells: {_info.NumCells}");
            
            if (_info.NumCells > 0)
            {
                var landblock = _info.Id & 0xFFFF0000;

                for (var i = 0; i < _info.NumCells; i++)
                {
                    var objCellID = landblock + 0x100 + i;
                    numCells.Items.Add(new TreeNode($"{objCellID:X8}", clickable: true));
                }
            }

            treeView.Items.Add(numCells);

            if (_info.Objects.Count != 0)
            {
                var objects = new TreeNode("Objects:");
                foreach (var stab in _info.Objects)
                {
                    var stabTree = new Stab(stab).BuildTree();
                    var obj = new TreeNode($"{stabTree[0].Name.Replace("ID: ", "")}", clickable: true);
                    stabTree.RemoveAt(0);
                    obj.Items.AddRange(stabTree);

                    objects.Items.Add(obj);
                }
                treeView.Items.Add(objects);
            }

            /*if (_info.PackMask != 0)
            {
                var packMask = new TreeNode($"PackMask: {_info.PackMask:X8}");
                treeView.Items.Add(packMask);
            }*/

            if (_info.Buildings.Count != 0)
            {
                var buildings = new TreeNode($"Buildings:");
                for (var i = 0; i < _info.Buildings.Count; i++)
                {
                    var building = new TreeNode($"{i}");
                    building.Items.AddRange(new BuildInfo(_info.Buildings[i]).BuildTree());
                    buildings.Items.Add(building);
                }
                treeView.Items.Add(buildings);
            }

            if (_info.RestrictionTables.Count != 0)
            {
                var restrictions = new TreeNode($"Restrictions:");
                foreach (var kvp in _info.RestrictionTables)
                    restrictions.Items.Add(new TreeNode($"{kvp.Key:X8}: {kvp.Value:X8}"));

                treeView.Items.Add(restrictions);
            }

            return treeView;
        }
    }
}

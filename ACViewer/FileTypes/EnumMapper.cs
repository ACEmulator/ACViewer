using System.Linq;

using ACE.Entity.Enum;

using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class EnumMapper
    {
        public ACE.DatLoader.FileTypes.EnumMapper _enumMapper;

        public EnumMapper(ACE.DatLoader.FileTypes.EnumMapper enumMapper)
        {
            _enumMapper = enumMapper;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_enumMapper.Id:X8}");

            if (_enumMapper.BaseEnumMap != 0)
            {
                var baseEnumMap = new TreeNode($"BaseEnumMap: {_enumMapper.BaseEnumMap:X8}");
                treeView.Items.Add(baseEnumMap);
            }

            if (_enumMapper.NumberingType != NumberingType.Undefined)
            {
                var numberingType = new TreeNode($"NumberingType: {_enumMapper.NumberingType}");
                treeView.Items.Add(numberingType);
            }

            if (_enumMapper.IdToStringMap.Count > 0)
            {
                var idToStringMap = new TreeNode("IdToStringMap:");
                foreach (var key in _enumMapper.IdToStringMap.Keys.OrderBy(i => i))
                    idToStringMap.Items.Add(new TreeNode($"{key}: {_enumMapper.IdToStringMap[key]}"));

                treeView.Items.Add(idToStringMap);
            }
            return treeView;
        }
    }
}

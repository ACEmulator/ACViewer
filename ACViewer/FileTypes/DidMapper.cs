using System.Collections.Generic;
using System.Linq;

using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class DidMapper
    {
        public ACE.DatLoader.FileTypes.DidMapper _didMapper;

        public DidMapper(ACE.DatLoader.FileTypes.DidMapper didMapper)
        {
            _didMapper = didMapper;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_didMapper.Id:X8}");

            if (_didMapper.ClientEnumToID.Count > 0)
            {
                var clientIDNumberingType = new TreeNode($"ClientIDNumberingType: {_didMapper.ClientIDNumberingType}");

                var clientEnumToID = new TreeNode("ClientEnumToID:");
                foreach (var key in _didMapper.ClientEnumToID.Keys.OrderBy(i => i))
                    clientEnumToID.Items.Add(new TreeNode($"{key:X8}: {_didMapper.ClientEnumToID[key]:X8}"));

                treeView.Items.AddRange(new List<TreeNode>() { clientIDNumberingType, clientEnumToID });
            }

            if (_didMapper.ClientEnumToName.Count > 0)
            {
                var clientNameNumberingType = new TreeNode($"ClientNameNumberingType: {_didMapper.ClientNameNumberingType}");

                var clientEnumToName = new TreeNode("ClientEnumToName:");
                foreach (var key in _didMapper.ClientEnumToName.Keys.OrderBy(i => i))
                    clientEnumToName.Items.Add(new TreeNode($"{key:X8}: {_didMapper.ClientEnumToName[key]:X8}"));

                treeView.Items.AddRange(new List<TreeNode>() { clientNameNumberingType, clientEnumToName });
            }

            if (_didMapper.ServerEnumToID.Count > 0)
            {
                var serverIDNumberingType = new TreeNode($"ServerIDNumberingType: {_didMapper.ServerIDNumberingType}");

                var serverEnumToID = new TreeNode("ServerEnumToID:");
                foreach (var key in _didMapper.ServerEnumToID.Keys.OrderBy(i => i))
                    serverEnumToID.Items.Add(new TreeNode($"{key:X8}: {_didMapper.ServerEnumToID[key]:X8}"));

                treeView.Items.AddRange(new List<TreeNode>() { serverIDNumberingType, serverEnumToID });
            }

            if (_didMapper.ServerEnumToName.Count > 0)
            {
                var serverNameNumberingType = new TreeNode($"ServerNameNumberingType: {_didMapper.ServerNameNumberingType}");

                var serverEnumToName = new TreeNode("ServerEnumToName:");
                foreach (var key in _didMapper.ServerEnumToName.Keys.OrderBy(i => i))
                    serverEnumToName.Items.Add(new TreeNode($"{key:X8}: {_didMapper.ServerEnumToName[key]:X8}"));

                treeView.Items.AddRange(new List<TreeNode>() { serverNameNumberingType, serverEnumToName });
            }

            return treeView;
        }
    }
}

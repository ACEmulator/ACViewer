using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class DualDidMapper
    {
        public ACE.DatLoader.FileTypes.DualDidMapper _dualDidMapper;

        public DualDidMapper(ACE.DatLoader.FileTypes.DualDidMapper dualDidMapper)
        {
            _dualDidMapper = dualDidMapper;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_dualDidMapper.Id:X8}");

            if (_dualDidMapper.ClientEnumToID.Count > 0)
            {
                var clientIDNumberingType = new TreeNode($"ClientIDNumberingType: {_dualDidMapper.ClientIDNumberingType}");

                var clientEnumToID = new TreeNode("ClientEnumToID:");
                foreach (var key in _dualDidMapper.ClientEnumToID.Keys.OrderBy(i => i))
                    clientEnumToID.Items.Add(new TreeNode($"{key:X8}: {_dualDidMapper.ClientEnumToID[key]:X8}"));

                treeView.Items.AddRange(new List<TreeNode>() { clientIDNumberingType, clientEnumToID });
            }

            if (_dualDidMapper.ClientEnumToName.Count > 0)
            {
                var clientNameNumberingType = new TreeNode($"ClientNameNumberingType: {_dualDidMapper.ClientNameNumberingType}");

                var clientEnumToName = new TreeNode("ClientEnumToName:");
                foreach (var key in _dualDidMapper.ClientEnumToName.Keys.OrderBy(i => i))
                    clientEnumToName.Items.Add(new TreeNode($"{key:X8}: {_dualDidMapper.ClientEnumToName[key]:X8}"));

                treeView.Items.AddRange(new List<TreeNode>() { clientNameNumberingType, clientEnumToName });
            }

            if (_dualDidMapper.ServerEnumToID.Count > 0)
            {
                var serverIDNumberingType = new TreeNode($"ServerIDNumberingType: {_dualDidMapper.ServerIDNumberingType}");

                var serverEnumToID = new TreeNode("ServerEnumToID:");
                foreach (var key in _dualDidMapper.ServerEnumToID.Keys.OrderBy(i => i))
                    serverEnumToID.Items.Add(new TreeNode($"{key:X8}: {_dualDidMapper.ServerEnumToID[key]:X8}"));

                treeView.Items.AddRange(new List<TreeNode>() { serverIDNumberingType, serverEnumToID });
            }

            if (_dualDidMapper.ServerEnumToName.Count > 0)
            {
                var serverNameNumberingType = new TreeNode($"ServerNameNumberingType: {_dualDidMapper.ServerNameNumberingType}");

                var serverEnumToName = new TreeNode("ServerEnumToName:");
                foreach (var key in _dualDidMapper.ServerEnumToName.Keys.OrderBy(i => i))
                    serverEnumToName.Items.Add(new TreeNode($"{key:X8}: {_dualDidMapper.ServerEnumToName[key]:X8}"));

                treeView.Items.AddRange(new List<TreeNode>() { serverNameNumberingType, serverEnumToName });
            }

            return treeView;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity.Enum;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class ContractTable
    {
        public ACE.DatLoader.FileTypes.ContractTable _contractTable;

        public ContractTable(ACE.DatLoader.FileTypes.ContractTable contractTable)
        {
            _contractTable = contractTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_contractTable.Id:X8}");

            foreach (var kvp in _contractTable.Contracts)
            {
                var contract = new TreeNode($"{kvp.Key} - {kvp.Value.ContractName}");
                contract.Items = new Contract(kvp.Value).BuildTree();
                treeView.Items.Add(contract);
            }
            return treeView;
        }
    }
}

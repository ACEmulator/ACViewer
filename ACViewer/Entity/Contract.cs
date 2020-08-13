using ACE.Entity.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class Contract
    {
        public ACE.DatLoader.Entity.Contract _contract;

        public Contract(ACE.DatLoader.Entity.Contract contract)
        {
            _contract = contract;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            treeNode.Add(new TreeNode($"ContractId: {_contract.ContractId}"));
            treeNode.Add(new TreeNode($"ContractName: {_contract.ContractName}"));
            treeNode.Add(new TreeNode($"Version: {_contract.Version}"));
            treeNode.Add(new TreeNode($"Description: {_contract.Description}"));
            treeNode.Add(new TreeNode($"DescriptionProgress: {_contract.DescriptionProgress}"));
            treeNode.Add(new TreeNode($"NameNPCStart: {_contract.NameNPCStart}"));
            treeNode.Add(new TreeNode($"NameNPCEnd: {_contract.NameNPCEnd}"));
            treeNode.Add(new TreeNode($"QuestflagStamped: {_contract.QuestflagStamped}"));
            treeNode.Add(new TreeNode($"QuestflagStarted: {_contract.QuestflagStarted}"));
            treeNode.Add(new TreeNode($"QuestflagFinished: {_contract.QuestflagFinished}"));
            treeNode.Add(new TreeNode($"QuestflagProgress: {_contract.QuestflagProgress}"));
            treeNode.Add(new TreeNode($"QuestflagTimer: {_contract.QuestflagTimer}"));
            treeNode.Add(new TreeNode($"QuestflagRepeatTime: {_contract.QuestflagRepeatTime}"));

            var locationNPCStart = new TreeNode($"LocationNPCStart");
            locationNPCStart.Items = new Position(_contract.LocationNPCStart).BuildTree();
            treeNode.Add(locationNPCStart);

            var locationNPCEnd = new TreeNode($"LocationNPCEnd");
            locationNPCEnd.Items = new Position(_contract.LocationNPCEnd).BuildTree();
            treeNode.Add(locationNPCEnd);

            var locationQuestArea = new TreeNode($"LocationQuestArea");
            locationQuestArea.Items = new Position(_contract.LocationQuestArea).BuildTree();
            treeNode.Add(locationQuestArea);

            return treeNode;
        }
    }
}

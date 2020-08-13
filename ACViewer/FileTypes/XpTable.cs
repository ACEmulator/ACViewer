using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class XpTable
    {
        public ACE.DatLoader.FileTypes.XpTable _xpTable;

        public XpTable(ACE.DatLoader.FileTypes.XpTable xpTable)
        {
            _xpTable = xpTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_xpTable.Id:X8}");

            var attributeXpList = new TreeNode("AttributeXpList");

            for (var i = 0; i < _xpTable.AttributeXpList.Count; i++)
            {
                var attributeXpNode = new TreeNode($"{i}: {_xpTable.AttributeXpList[i]:N0}");
                attributeXpList.Items.Add(attributeXpNode);
            }

            var vitalXpList = new TreeNode("VitalXpList");

            for (var i = 0; i < _xpTable.VitalXpList.Count; i++)
            {
                var vitalXpNode = new TreeNode($"{i}: {_xpTable.VitalXpList[i]:N0}");
                vitalXpList.Items.Add(vitalXpNode);
            }

            var trainedSkillXpList = new TreeNode("TrainedSkillXpList");

            for (var i = 0; i < _xpTable.TrainedSkillXpList.Count; i++)
            {
                var trainedSkillXpNode = new TreeNode($"{i}: {_xpTable.TrainedSkillXpList[i]:N0}");
                trainedSkillXpList.Items.Add(trainedSkillXpNode);
            }

            var specializedSkillXpList = new TreeNode("SpecializedSkillXpList");

            for (var i = 0; i < _xpTable.SpecializedSkillXpList.Count; i++)
            {
                var specializedSkillXpNode = new TreeNode($"{i}: {_xpTable.SpecializedSkillXpList[i]:N0}");
                specializedSkillXpList.Items.Add(specializedSkillXpNode);
            }

            treeView.Items.AddRange(new List<TreeNode>() { attributeXpList, vitalXpList, trainedSkillXpList, specializedSkillXpList });

            return treeView;
        }
    }
}

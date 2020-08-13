using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class ChatPoseTable
    {
        public ACE.DatLoader.FileTypes.ChatPoseTable _chatPoseTable;

        public ChatPoseTable(ACE.DatLoader.FileTypes.ChatPoseTable chatPoseTable)
        {
            _chatPoseTable = chatPoseTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_chatPoseTable.Id:X8}");

            var chatPoseHash = new TreeNode($"ChatPoseHash");

            foreach (var kvp in _chatPoseTable.ChatPoseHash.OrderBy(i => i.Key))
                chatPoseHash.Items.Add(new TreeNode($"{kvp.Key}: {kvp.Value}"));

            var chatEmoteHash = new TreeNode($"ChatEmoteHash");

            foreach (var kvp in _chatPoseTable.ChatEmoteHash.OrderBy(i => i.Key))
            {
                var chatEmote = new TreeNode(kvp.Key);
                chatEmote.Items = new ChatEmoteData(kvp.Value).BuildTree();

                chatEmoteHash.Items.Add(chatEmote);
            }
            treeView.Items = new List<TreeNode>() { chatPoseHash, chatEmoteHash };

            return treeView;
        }
    }
}

using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class ChatEmoteData
    {
        public ACE.DatLoader.Entity.ChatEmoteData _chatEmoteData;

        public ChatEmoteData(ACE.DatLoader.Entity.ChatEmoteData chatEmoteData)
        {
            _chatEmoteData = chatEmoteData;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            treeNode.Add(new TreeNode($"MyEmote: {_chatEmoteData.MyEmote}"));
            treeNode.Add(new TreeNode($"OtherEmote: {_chatEmoteData.OtherEmote}"));
            
            return treeNode;
        }
    }
}

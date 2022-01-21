using System.Collections.Generic;

using ACE.Entity.Enum;

namespace ACViewer.Entity
{
    public class SpellComponentBase
    {
        public ACE.DatLoader.Entity.SpellComponentBase _spellComponentBase;

        public SpellComponentBase(ACE.DatLoader.Entity.SpellComponentBase spellComponentBase)
        {
            _spellComponentBase = spellComponentBase;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            treeNode.Add(new TreeNode($"Name: {_spellComponentBase.Name}"));
            treeNode.Add(new TreeNode($"Category: {_spellComponentBase.Category}"));
            treeNode.Add(new TreeNode($"Icon: {_spellComponentBase.Icon:X8}", clickable: true));
            treeNode.Add(new TreeNode($"Type: {(ACE.DatLoader.FileTypes.SpellComponentsTable.Type)_spellComponentBase.Type}"));

            var gesture = _spellComponentBase.Gesture == 0x80000000 ? "Style" : $"{(MotionCommand)_spellComponentBase.Gesture}";
            
            if (!gesture.Equals("Style"))
                treeNode.Add(new TreeNode($"Gesture: {gesture}"));

            treeNode.Add(new TreeNode($"Time: {_spellComponentBase.Time}"));
            
            if (!string.IsNullOrEmpty(_spellComponentBase.Text))
                treeNode.Add(new TreeNode($"Text: {_spellComponentBase.Text}"));
            
            treeNode.Add(new TreeNode($"CDM: {_spellComponentBase.CDM}"));

            return treeNode;
        }
    }
}

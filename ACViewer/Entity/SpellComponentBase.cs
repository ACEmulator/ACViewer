using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using ACE.DatLoader;
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
            treeNode.Add(new TreeNode($"Icon: 0x{_spellComponentBase.Icon:X8}"));
            treeNode.Add(new TreeNode($"Type: {(ACE.DatLoader.FileTypes.SpellComponentsTable.Type)_spellComponentBase.Type}"));

            var gesture = _spellComponentBase.Gesture == 0x80000000 ? $"0x{_spellComponentBase.Gesture:X8}" : $"{(MotionCommand)_spellComponentBase.Gesture}";
            
            treeNode.Add(new TreeNode($"Gesture: {gesture}"));

            treeNode.Add(new TreeNode($"Time: {_spellComponentBase.Time}"));
            treeNode.Add(new TreeNode($"Text: {_spellComponentBase.Text}"));
            treeNode.Add(new TreeNode($"CDM: {_spellComponentBase.CDM}"));

            return treeNode;
        }
    }
}

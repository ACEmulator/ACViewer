using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class NameFilterLanguageData
    {
        public ACE.DatLoader.Entity.NameFilterLanguageData _nameFilterLanguageData;

        public NameFilterLanguageData(ACE.DatLoader.Entity.NameFilterLanguageData nameFilterLanguageData)
        {
            _nameFilterLanguageData = nameFilterLanguageData;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            treeNode.Add(new TreeNode($"MaximumVowelsInARow: {_nameFilterLanguageData.MaximumVowelsInARow}"));
            treeNode.Add(new TreeNode($"FirstNCharactersMustHaveAVowel: {_nameFilterLanguageData.FirstNCharactersMustHaveAVowel}"));
            treeNode.Add(new TreeNode($"VowelContainingSubstringLength: {_nameFilterLanguageData.VowelContainingSubstringLength}"));
            treeNode.Add(new TreeNode($"ExtraAllowedCharacters: {_nameFilterLanguageData.ExtraAllowedCharacters}"));
            treeNode.Add(new TreeNode($"Unknown: {_nameFilterLanguageData.Unknown}"));

            var compoundLetterGroups = new TreeNode($"CompoundLetterGrounds");

            foreach (var compoundLetterGroup in _nameFilterLanguageData.CompoundLetterGroups)
                compoundLetterGroups.Items.Add(new TreeNode(compoundLetterGroup));

            treeNode.Add(compoundLetterGroups);

            return treeNode;
        }
    }
}

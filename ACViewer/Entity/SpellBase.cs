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
    public class SpellBase
    {
        public ACE.DatLoader.Entity.SpellBase _spellBase;

        public SpellBase(ACE.DatLoader.Entity.SpellBase spellBase)
        {
            _spellBase = spellBase;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            treeNode.Add(new TreeNode($"Name: {_spellBase.Name}"));
            treeNode.Add(new TreeNode($"Description: {_spellBase.Desc}"));
            treeNode.Add(new TreeNode($"School: {_spellBase.School}"));
            treeNode.Add(new TreeNode($"Icon: 0x{_spellBase.Icon:X8}"));
            treeNode.Add(new TreeNode($"Category: {_spellBase.Category}"));
            treeNode.Add(new TreeNode($"Flags: {(SpellFlags)_spellBase.Bitfield}"));
            treeNode.Add(new TreeNode($"BaseMana: {_spellBase.BaseMana}"));
            treeNode.Add(new TreeNode($"BaseRangeConstant: {_spellBase.BaseRangeConstant}"));
            treeNode.Add(new TreeNode($"BaseRangeMod: {_spellBase.BaseRangeMod}"));
            treeNode.Add(new TreeNode($"Power: {_spellBase.Power}"));
            treeNode.Add(new TreeNode($"SpellEconomyMod: {_spellBase.SpellEconomyMod}"));
            treeNode.Add(new TreeNode($"FormulaVersion: {_spellBase.FormulaVersion}"));
            treeNode.Add(new TreeNode($"ComponentLoss: {_spellBase.ComponentLoss}"));
            treeNode.Add(new TreeNode($"MetaSpellType: {_spellBase.MetaSpellType}"));
            treeNode.Add(new TreeNode($"MetaSpellId: {_spellBase.MetaSpellId}"));
            treeNode.Add(new TreeNode($"Duration: {_spellBase.Duration}"));
            treeNode.Add(new TreeNode($"DegradeModifier: {_spellBase.DegradeModifier}"));
            treeNode.Add(new TreeNode($"DegradeLimit: {_spellBase.DegradeLimit}"));

            var formula = new TreeNode("Formula");
            foreach (var componentId in _spellBase.Formula)
            {
                var component = DatManager.PortalDat.SpellComponentsTable.SpellComponents[componentId];
                formula.Items.Add(new TreeNode($"{componentId}: {component.Name}"));
            }

            treeNode.Add(formula);
            treeNode.Add(new TreeNode($"CasterEffect: {(PlayScript)_spellBase.CasterEffect}"));
            treeNode.Add(new TreeNode($"TargetEffect: {(PlayScript)_spellBase.TargetEffect}"));
            treeNode.Add(new TreeNode($"FizzleEffect: {(PlayScript)_spellBase.FizzleEffect}"));
            treeNode.Add(new TreeNode($"RecoveryInterval: {_spellBase.RecoveryInterval}"));
            treeNode.Add(new TreeNode($"RecoveryAmount: {_spellBase.RecoveryAmount}"));
            treeNode.Add(new TreeNode($"DisplayOrder: {_spellBase.DisplayOrder}"));
            treeNode.Add(new TreeNode($"NonComponentTargetType: {(ItemType)_spellBase.NonComponentTargetType}"));
            treeNode.Add(new TreeNode($"ManaMod: {_spellBase.ManaMod}"));

            return treeNode;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class SexCG
    {
        public ACE.DatLoader.Entity.SexCG _sex;

        public SexCG(ACE.DatLoader.Entity.SexCG sex)
        {
            _sex = sex;
        }

        public List<TreeNode> BuildTree()
        {
            var name = new TreeNode($"Name: {_sex.Name}");
            var scale = new TreeNode($"Scale: {_sex.Scale}%");
            var setup = new TreeNode($"Setup: {_sex.SetupID:X8}");
            var soundTable = new TreeNode($"SoundTable: {_sex.SoundTable:X8}");
            var icon = new TreeNode($"Icon: {_sex.IconImage:X8}");
            var basePalette = new TreeNode($"Base Palette: {_sex.BasePalette:X8}");
            var skinPaletteSet = new TreeNode($"Skin Palette Set: {_sex.SkinPalSet:X8}");
            var physicsTable = new TreeNode($"Physics Table: {_sex.PhysicsTable:X8}");
            var motionTable = new TreeNode($"Motion Table: {_sex.MotionTable:X8}");
            var combatTable = new TreeNode($"Combat Table: {_sex.CombatTable:X8}");
            var baseObjDesc = new TreeNode("ObjDesc:");
            baseObjDesc.Items.AddRange(new ObjDesc(_sex.BaseObjDesc).BuildTree());

            var hairColors = new TreeNode($"Hair Colors:");
            foreach (var hairColor in _sex.HairColorList)
                hairColors.Items.Add(new TreeNode($"{hairColor:X8}"));

            var hairStyles = new TreeNode($"Hair Styles:");
            for (var i = 0; i < _sex.HairStyleList.Count; i++)
            {
                var hairStyle = new TreeNode($"{i}");
                hairStyle.Items.AddRange(new HairStyleCG(_sex.HairStyleList[i]).BuildTree());
                hairStyles.Items.Add(hairStyle);
            }

            var eyeColors = new TreeNode($"Eye Colors:");
            foreach (var eyeColor in _sex.EyeColorList)
                eyeColors.Items.Add(new TreeNode($"{eyeColor:X8}"));

            var eyeStrips = new TreeNode($"Eye Strips:");
            for (var i = 0; i < _sex.EyeStripList.Count; i++)
            {
                var eyeStrip = new TreeNode($"{i}");
                eyeStrip.Items.AddRange(new EyeStripCG(_sex.EyeStripList[i]).BuildTree());
                eyeStrips.Items.Add(eyeStrip);
            }

            var noseStrips = new TreeNode($"Nose Strips:");
            for (var i = 0; i < _sex.NoseStripList.Count; i++)
            {
                var noseStrip = new TreeNode($"{i}");
                noseStrip.Items.AddRange(new FaceStripCG(_sex.NoseStripList[i]).BuildTree());
                noseStrips.Items.Add(noseStrip);
            }

            var mouthStrips = new TreeNode($"Mouth Strips:");
            for (var i = 0; i < _sex.MouthStripList.Count; i++)
            {
                var mouthStrip = new TreeNode($"{i}");
                mouthStrip.Items.AddRange(new FaceStripCG(_sex.MouthStripList[i]).BuildTree());
                mouthStrips.Items.Add(mouthStrip);
            }

            var headgear = new TreeNode($"Headgear:");
            for (var i = 0; i < _sex.HeadgearList.Count; i++)
            {
                var headgearNode = new TreeNode($"{i}");
                headgearNode.Items.AddRange(new GearCG(_sex.HeadgearList[i]).BuildTree());
                headgear.Items.Add(headgearNode);
            }

            var shirts = new TreeNode($"Shirt:");
            for (var i = 0; i < _sex.ShirtList.Count; i++)
            {
                var shirt = new TreeNode($"{i}");
                shirt.Items.AddRange(new GearCG(_sex.ShirtList[i]).BuildTree());
                shirts.Items.Add(shirt);
            }

            var pants = new TreeNode($"Pants:");
            for (var i = 0; i < _sex.PantsList.Count; i++)
            {
                var pantsNode = new TreeNode($"{i}");
                pantsNode.Items.AddRange(new GearCG(_sex.PantsList[i]).BuildTree());
                pants.Items.Add(pantsNode);
            }

            var footwear = new TreeNode($"Footwear:");
            for (var i = 0; i < _sex.FootwearList.Count; i++)
            {
                var footwearNode = new TreeNode($"{i}");
                footwearNode.Items.AddRange(new GearCG(_sex.FootwearList[i]).BuildTree());
                footwear.Items.Add(footwearNode);
            }

            var clothingColors = new TreeNode($"Clothing Colors: {String.Join(",", _sex.ClothingColorsList)}");

            return new List<TreeNode>() { name, scale, setup, soundTable, icon, basePalette, skinPaletteSet, physicsTable, motionTable, combatTable,
                baseObjDesc, hairColors, hairStyles, eyeColors, eyeStrips, noseStrips, mouthStrips, headgear, shirts, pants, footwear, clothingColors };
        }
    }
}

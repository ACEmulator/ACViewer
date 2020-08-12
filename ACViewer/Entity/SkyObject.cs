using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class SkyObject
    {
        public ACE.DatLoader.Entity.SkyObject _skyObject;

        public SkyObject(ACE.DatLoader.Entity.SkyObject skyObject)
        {
            _skyObject = skyObject;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_skyObject.BeginTime != 0)
            {
                var beginTime = new TreeNode($"BeginTime: {_skyObject.BeginTime}");
                treeNode.Add(beginTime);
            }

            if (_skyObject.EndTime != 0)
            {
                var endTime = new TreeNode($"EndTime: {_skyObject.EndTime}");
                treeNode.Add(endTime);
            }

            if (_skyObject.BeginAngle != 0)
            {
                var beginAngle = new TreeNode($"BeginAngle: {_skyObject.BeginAngle}");
                treeNode.Add(beginAngle);
            }

            if (_skyObject.EndAngle != 0)
            {
                var endAngle = new TreeNode($"EndAngle: {_skyObject.EndAngle}");
                treeNode.Add(endAngle);
            }

            if (_skyObject.TexVelocityX != 0)
            {
                var texVelocityX = new TreeNode($"TextureVelocityX: {_skyObject.TexVelocityX}");
                treeNode.Add(texVelocityX);
            }

            if (_skyObject.TexVelocityY != 0)
            {
                var texVelocityY = new TreeNode($"TextureVelocityY: {_skyObject.TexVelocityY}");
                treeNode.Add(texVelocityY);
            }

            if (_skyObject.DefaultGFXObjectId != 0)
            {
                var defaultGfxObjID = new TreeNode($"DefaultGfxObjID: {_skyObject.DefaultGFXObjectId:X8}");
                treeNode.Add(defaultGfxObjID);
            }

            if (_skyObject.DefaultPESObjectId != 0)
            {
                var defaultPESObjID = new TreeNode($"DefaultPESObjID: {_skyObject.DefaultPESObjectId:X8}");
                treeNode.Add(defaultPESObjID);
            }

            if (_skyObject.Properties != 0)
            {
                var properties = new TreeNode($"Properties: 0x{_skyObject.Properties:X}");
                treeNode.Add(properties);
            }

            return treeNode;
        }
    }
}

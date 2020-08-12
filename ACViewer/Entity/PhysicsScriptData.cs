using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class PhysicsScriptData
    {
        public ACE.DatLoader.Entity.PhysicsScriptData _scriptData;

        public PhysicsScriptData(ACE.DatLoader.Entity.PhysicsScriptData scriptData)
        {
            _scriptData = scriptData;
        }

        public List<TreeNode> BuildTree()
        {
            var startTime = new TreeNode($"StartTime: {_scriptData.StartTime}");
            var hook = new TreeNode($"{new AnimationHook(_scriptData.Hook).ToString()}");

            return new List<TreeNode>() { startTime, hook };
        }
    }
}

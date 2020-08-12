using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class Stab
    {
        public ACE.DatLoader.Entity.Stab _stab;

        public Stab(ACE.DatLoader.Entity.Stab stab)
        {
            _stab = stab;
        }

        public List<TreeNode> BuildTree()
        {
            var id = new TreeNode($"ID: {_stab.Id:X8}");
            var frame = new TreeNode($"Frame: {new Frame(_stab.Frame).ToString()}");

            return new List<TreeNode>() { id, frame };
        }

        public override string ToString()
        {
            return $"ID: {_stab.Id:X8}, Frame: {new Frame(_stab.Frame).ToString()}";
        }
    }
}

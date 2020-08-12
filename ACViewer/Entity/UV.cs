using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ACViewer.Entity
{
    public class UV
    {
        public ACE.DatLoader.Entity.Vec2Duv _uv;

        public UV(ACE.DatLoader.Entity.Vec2Duv uv)
        {
            _uv = uv;
        }

        public TreeNode BuildTree()
        {
            return new TreeNode($"U: {_uv.U} V: {_uv.V}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer
{ 
    public static class MatrixExtensions
    {
        public static System.Numerics.Matrix4x4 ToNumerics(this Microsoft.Xna.Framework.Matrix m)
        {
            return new System.Numerics.Matrix4x4(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
        }
    }
}

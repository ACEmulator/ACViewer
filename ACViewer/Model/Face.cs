using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ACViewer.Model
{
    public class Face
    {
        public Facing Facing;

        public Vector3 NW;
        public Vector3 NE;
        public Vector3 SE;
        public Vector3 SW;

        public Vector3 Normal;
        public Vector3 Center;

        public float Width;
        public float Height;

        public float Area;

        public bool GfxObjMode { get => ModelViewer.Instance.GfxObjMode; }

        public Face(Facing facing, Vector3 nw, Vector3 ne, Vector3 se, Vector3 sw, Vector3 normal)
        {
            Facing = facing;

            NW = nw;
            NE = ne;
            SE = se;
            SW = sw;
            Normal = normal;

            Center = new Vector3(nw.X, nw.Y, nw.Z);

            if (facing == Facing.Front || Facing == Facing.Back)
            {
                Width = Math.Abs(ne.X - nw.X);
                Height = Math.Abs(ne.Z - se.Z);

                if (GfxObjMode)
                    Center.X += Width * 0.5f;
                else
                    Center.X -= Width * 0.5f;

                Center.Z -= Height * 0.5f;
            }
            else if (facing == Facing.Left || Facing == Facing.Right)
            {
                Width = Math.Abs(ne.Y - nw.Y);
                Height = Math.Abs(ne.Z - se.Z);

                if (GfxObjMode)
                    Center.Y -= Width * 0.5f;
                else
                    Center.Y += Width * 0.5f;

                Center.Z -= Height * 0.5f;
            }
            else if (facing == Facing.Top || Facing == Facing.Bottom)
            {
                Width = Math.Abs(ne.X - nw.X);
                Height = Math.Abs(ne.Y - se.Y);

                Center.X -= Width * 0.5f;
                Center.Y += Height * 0.5f;
            }

            Area = Width * Height;
        }
    }
}

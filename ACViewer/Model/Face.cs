using System;

using Microsoft.Xna.Framework;

using ACViewer.Enum;

namespace ACViewer.Model
{
    public class Face
    {
        public Facing Facing { get; set; }

        public Vector3 NW { get; set; }
        public Vector3 NE { get; set; }
        public Vector3 SE { get; set; }
        public Vector3 SW { get; set; }

        public Vector3 Normal { get; set; }
        public Vector3 Center { get; set; }

        public float Width { get; set; }
        public float Height { get; set; }

        public float Area { get; set; }

        public bool GfxObjMode => ModelViewer.Instance.GfxObjMode;

        public Face(Facing facing, Vector3 nw, Vector3 ne, Vector3 se, Vector3 sw, Vector3 normal)
        {
            Facing = facing;

            NW = nw;
            NE = ne;
            SE = se;
            SW = sw;
            Normal = normal;

            var center = new Vector3(nw.X, nw.Y, nw.Z);

            if (facing == Facing.Front || Facing == Facing.Back)
            {
                Width = Math.Abs(ne.X - nw.X);
                Height = Math.Abs(ne.Z - se.Z);

                if (GfxObjMode)
                    center.X += Width * 0.5f;
                else
                    center.X -= Width * 0.5f;

                center.Z -= Height * 0.5f;
            }
            else if (facing == Facing.Left || Facing == Facing.Right)
            {
                Width = Math.Abs(ne.Y - nw.Y);
                Height = Math.Abs(ne.Z - se.Z);

                if (GfxObjMode)
                    center.Y -= Width * 0.5f;
                else
                    center.Y += Width * 0.5f;

                center.Z -= Height * 0.5f;
            }
            else if (facing == Facing.Top || Facing == Facing.Bottom)
            {
                Width = Math.Abs(ne.X - nw.X);
                Height = Math.Abs(ne.Y - se.Y);

                center.X -= Width * 0.5f;
                center.Y += Height * 0.5f;
            }

            Center = center;

            Area = Width * Height;
        }
    }
}

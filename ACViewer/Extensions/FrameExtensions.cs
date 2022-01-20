using ACE.DatLoader.Entity;
using ACE.Server.Physics.Animation;

using Microsoft.Xna.Framework;

namespace ACViewer
{
    public static class FrameExtensions
    {
        public static Matrix ToXna(this Frame frame)
        {
            var translate = Matrix.CreateTranslation(frame.Origin.ToXna());
            var rotate = Matrix.CreateFromQuaternion(frame.Orientation.ToXna());

            return rotate * translate;
        }

        public static Matrix ToXna(this AFrame frame)
        {
            var translate = Matrix.CreateTranslation(frame.Origin.ToXna());
            var rotate = Matrix.CreateFromQuaternion(frame.Orientation.ToXna());

            return rotate * translate;
        }
    }
}

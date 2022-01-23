using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.Server.Physics;

using ACViewer.Model;

namespace ACViewer.Render
{
    public class R_PhysicsObj
    {
        public GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;
        public Effect Effect => Render.Effect;

        public PhysicsObj PhysicsObj { get; set; }
        public R_PartArray PartArray { get; set; }

        public SetupInstance Setup { get; set; }

        public R_PhysicsObj(PhysicsObj obj)
        {
            PhysicsObj = obj;
            PartArray = new R_PartArray(obj.PartArray);
            Setup = new SetupInstance(this);
        }

        public void Draw(Matrix world)
        {
            GraphicsDevice.SetVertexBuffer(PartArray.VertexBuffer);
            GraphicsDevice.Indices = PartArray.IndexBuffer;

            for (var i = 0; i < PartArray.Parts.Count; i++)
            {
                var part = PartArray.Parts[i];

                var rotate = Matrix.CreateFromQuaternion(part.PhysicsPart.Pos.Frame.Orientation.ToXna());
                var translate = Matrix.CreateTranslation(part.PhysicsPart.Pos.Frame.Origin.ToXna());

                var transform = rotate * world * translate;

                Effect.Parameters["xWorld"].SetValue(transform);

                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, PartArray.VertexOffsets[i], PartArray.IndexOffsets[i], part.R_GfxObj.Indices.Length / 2);
                    //PerfTimer.NumPhysicsObj++;
                }
            }
        }
    }
}

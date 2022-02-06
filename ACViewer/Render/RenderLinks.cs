using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.Server.WorldObjects;

namespace ACViewer.Render
{
    public class RenderLinks
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public static Effect Effect => Render.Effect;
        
        public LinkNode Head { get; set; }

        public LinkNode Selected { get; set; }

        public List<VertexPositionColor> LineVerts { get; set; }

        public List<VertexPositionColor> ArrowVerts { get; set; }

        public List<VertexPositionColor> RadiusVerts { get; set; }

        public List<short> RadiusIndices { get; set; }

        public VertexBuffer Lines_VB { get; set; }

        public VertexBuffer Arrows_VB { get; set; }

        public VertexBuffer Radius_VB { get; set; }

        public IndexBuffer Radius_IB { get; set; }

        public RenderLinks(LinkNode node)
        {
            Selected = node;
            
            Head = node.GetHeadNode();

            BuildVertices();
        }

        public void BuildVertices()
        {
            LineVerts = new List<VertexPositionColor>();
            ArrowVerts = new List<VertexPositionColor>();
            
            AddChildTree(Head);

            if (LineVerts != null && LineVerts.Count > 0)
            {
                Lines_VB = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), LineVerts.Count, BufferUsage.WriteOnly);
                Lines_VB.SetData(LineVerts.ToArray());

                Arrows_VB = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), ArrowVerts.Count, BufferUsage.WriteOnly);
                Arrows_VB.SetData(ArrowVerts.ToArray());
            }

            AddRadius(Selected);

            if (RadiusVerts != null && RadiusVerts.Count > 0)
            {
                Radius_VB = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), RadiusVerts.Count, BufferUsage.WriteOnly);
                Radius_VB.SetData(RadiusVerts.ToArray());

                Radius_IB = new IndexBuffer(GraphicsDevice, typeof(short), RadiusIndices.Count, BufferUsage.WriteOnly);
                Radius_IB.SetData(RadiusIndices.ToArray());
            }
        }

        private static readonly int RadiusPoints = 64;

        public void AddRadius(LinkNode selected)
        {
            if (!selected.WorldObject.IsGenerator || selected.WorldObject.GeneratorRadius == null) return;

            var radius = (float)selected.WorldObject.GeneratorRadius;

            var startPos = selected.WorldObject.PhysicsObj.Position.GetWorldPos();

            RadiusVerts = new List<VertexPositionColor>();

            var stepSize = (float)Math.PI * 2 / RadiusPoints;
            
            for (var i = 0; i < RadiusPoints; i++)
            {
                var q = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, i * stepSize);
                var local = Vector3.Transform(Vector3.UnitY * radius, q);
                var world = startPos + local;

                RadiusVerts.Add(new VertexPositionColor(world, Color.OrangeRed));
            }

            RadiusIndices = new List<short>();

            for (short i = 0; i < RadiusVerts.Count; i++)
            {
                RadiusIndices.Add(i);

                if (i < RadiusVerts.Count - 1)
                    RadiusIndices.Add((short)(i + 1));
                else
                    RadiusIndices.Add(0);
            }
        }

        public static Color LinkColor = Color.OrangeRed;
        //public static Color LinkColor = Color.LimeGreen;

        private static readonly float arrowLength = 0.25f;

        public bool AddChildTree(LinkNode node)
        {
            if (node.Children == null || node.WorldObject.PhysicsObj == null) return false;
            
            foreach (var child in node.Children)
            {
                // debug me: click on the pathwarden chest in yaraq
                if (child.WorldObject.PhysicsObj == null) continue;
                
                var parentLoc = node.WorldObject.PhysicsObj.Position.GetWorldPos();
                var childLoc = child.WorldObject.PhysicsObj.Position.GetWorldPos();

                if (!(node.WorldObject is PressurePlate))
                    parentLoc.Z += node.WorldObject.PhysicsObj.GetHeight() * 0.67f;

                if (!(child.WorldObject is PressurePlate))
                    childLoc.Z += child.WorldObject.PhysicsObj.GetHeight() * 0.67f;

                var childToParentDir = Vector3.Normalize(parentLoc - childLoc);

                //var parentRad = node.WorldObject.PhysicsObj.GetPhysicsRadius();
                //parentLoc -= childToParentDir * parentRad * 0.67f;
                
                if (Picker.PickResult.PhysicsObj == child.WorldObject.PhysicsObj)
                {
                    var childRad = child.WorldObject.PhysicsObj.GetPhysicsRadius();
                    childLoc += childToParentDir * childRad * 0.67f;
                }

                // add line
                LineVerts.Add(new VertexPositionColor(parentLoc, LinkColor));
                LineVerts.Add(new VertexPositionColor(childLoc, LinkColor));

                // add arrowhead
                var ah1 = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.PI / 6.0f));
                var ah2 = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -(float)(Math.PI / 6.0f));

                ArrowVerts.Add(new VertexPositionColor(childLoc, LinkColor));
                ArrowVerts.Add(new VertexPositionColor(Vector3.Transform(childToParentDir * arrowLength, ah1) + childLoc, LinkColor));
                ArrowVerts.Add(new VertexPositionColor(Vector3.Transform(childToParentDir * arrowLength, ah2) + childLoc, LinkColor));

                AddChildTree(child);    // recurse
            }
            return true;
        }

        public void Draw()
        {
            if ((LineVerts == null || LineVerts.Count == 0) && (RadiusVerts == null || RadiusVerts.Count == 0)) return;

            var rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rs;

            Effect.CurrentTechnique = Effect.Techniques["Picker"];

            if (LineVerts != null && LineVerts.Count > 0)
            {
                GraphicsDevice.SetVertexBuffer(Lines_VB);

                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, LineVerts.Count / 2);
                }

                GraphicsDevice.SetVertexBuffer(Arrows_VB);

                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, ArrowVerts.Count / 3);
                }
            }

            if (RadiusVerts != null && RadiusVerts.Count > 0)
            {
                GraphicsDevice.SetVertexBuffer(Radius_VB);
                GraphicsDevice.Indices = Radius_IB;

                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, RadiusVerts.Count);
                }
            }
        }

        public void Dispose()
        {
            if (Lines_VB != null)
                Lines_VB.Dispose();

            if (Arrows_VB != null)
                Arrows_VB.Dispose();

            if (Radius_VB != null)
                Radius_VB.Dispose();

            if (Radius_IB != null)
                Radius_IB.Dispose();
        }
    }
}

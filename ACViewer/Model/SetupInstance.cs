using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACViewer.Render;
using System.Collections.Generic;

namespace ACViewer.Model
{
    public class SetupInstance
    {
        public static GraphicsDevice GraphicsDevice { get => GameView.Instance.GraphicsDevice; }
        public static Effect Effect { get => Render.Render.Effect; }
        public static Camera Camera { get => GameView.Camera; }

        public Setup Setup;

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public Matrix WorldTranslate;
        public Matrix WorldRotate;
        public Matrix WorldScale;

        public Matrix WorldTransform;

        public ViewObject ViewObject { get => ModelViewer.Instance.ViewObject; }

        public float Angle;

        public SetupInstance(uint setupID)
        {
            Setup = SetupCache.Get(setupID);

            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;

            BuildWorldTransform();
        }

        /// <summary>
        /// For loading a SetupInstance with a Clothing Base
        /// </summary>
        public SetupInstance(uint setupID, FileTypes.ObjDesc objDesc, Dictionary<int, uint> customPaletteColors)
        {
            Setup = new Setup(setupID, objDesc, customPaletteColors);
            
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;

            BuildWorldTransform();
        }

        public SetupInstance(R_PhysicsObj obj)
        {
            var setupID = obj.PhysicsObj.PartArray.Setup._dat.Id;

            if (setupID == 0 && obj.PhysicsObj.PartArray.Parts.Count > 0)
                setupID = obj.PhysicsObj.PartArray.Parts[0].GfxObj.ID;

            if (setupID == 0)
                setupID = obj.PhysicsObj.ID;

            Setup = SetupCache.Get(setupID);

            Position = obj.PhysicsObj.Position.Frame.Origin.ToXna();
            Rotation = obj.PhysicsObj.Position.Frame.Orientation.ToXna();
            Scale = obj.PhysicsObj.PartArray.Scale.ToXna();

            BuildWorldTransform();
        }

        public void BuildWorldTransform()
        {
            WorldTranslate = Matrix.CreateTranslation(Position);
            WorldRotate = Matrix.CreateFromQuaternion(Rotation);
            WorldScale = Matrix.CreateScale(Scale);

            WorldTransform = WorldRotate * WorldScale * WorldTranslate;
        }

        public void SetRasterizerState()
        {
            var rs = new RasterizerState();
            //rs.CullMode = CullMode.CullClockwiseFace;
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rs;
        }

        public void Draw(int polyIdx = -1)
        {
            SetRasterizerState();

            var physParts = ViewObject.PhysicsObj.PartArray.Parts;

            var tris = 0;
            var curPolyIdx = 0;
            for (var i = 0; i < Setup.Parts.Count; i++)
            {
                //var placementFrame = Setup.PlacementFrames[i];
                var part = Setup.Parts[i];

                var partFrame = physParts[i].Pos.Frame;
                var partTransform = partFrame.ToXna();

                //var transform = placementFrame * partTransform * WorldTransform;
                var transform = partTransform * WorldTransform;

                if (part.VertexBuffer == null)
                    part.BuildVertexBuffer();

                GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                Effect.Parameters["xWorld"].SetValue(transform);

                foreach (var polygon in part.Polygons)
                {
                    if (polyIdx != -1 && polyIdx != curPolyIdx)
                    {
                        curPolyIdx++;
                        continue;
                    }

                    if (polygon.IndexBuffer == null)
                        polygon.BuildIndexBuffer();

                    GraphicsDevice.Indices = polygon.IndexBuffer;
                    Effect.Parameters["xTextures"].SetValue(polygon.Texture);

                    foreach (var pass in Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        var indexCnt = polygon.Indices.Count;
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexCnt / 3);
                        DrawCount.NumSetup++;
                        tris += indexCnt / 3;
                    }

                    curPolyIdx++;
                }
            }
            /*Console.WriteLine($"Drew {tris} triangles");
            Console.WriteLine($"Camera pos: {Camera.Position}");
            Console.WriteLine($"Camera dir: {Camera.Dir}");*/

            //UpdateAngle();
        }

        public void UpdateAngle()
        {
            Angle += 1.0f;
            if (Angle >= 360.0f)
                Angle = 0.0f;

            Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, Angle * 0.0174533f);
            BuildWorldTransform();
        }
    }
}

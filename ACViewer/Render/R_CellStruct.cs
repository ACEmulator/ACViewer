using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.DatLoader.Entity;
using ACE.DatLoader.FileTypes;

namespace ACViewer.Render
{
    public class R_CellStruct
    {
        public static GraphicsDevice GraphicsDevice { get => GameView.Instance.GraphicsDevice; }
        public static Effect Effect { get => Render.Effect; }

        public CellStruct CellStruct;

        public List<VertexPositionNormalTexture> VertexArray;
        public List<VertexPositionColor> W_VertexArray;
        public Dictionary<Tuple<ushort, ushort>, ushort> UVLookup;

        public List<Model.Polygon> Polygons;

        public VertexBuffer W_VertexBuffer;
        public VertexBuffer VertexBuffer;

        public List<ushort> Indices;
        public IndexBuffer IndexBuffer;

        public List<Texture2D> Textures;

        public List<Surface> Surfaces;

        public R_CellStruct(CellStruct cellStruct)
        {
            // caching?
            CellStruct = cellStruct;

            VertexArray = CellStruct.VertexArray.ToXna();
            W_VertexArray = CellStruct.VertexArray.ToWireframeXna();
            UVLookup = CellStruct.VertexArray.BuildUVLookup();

            // surfaces?

            BuildPolygons();
            //BuildVertexBuffer();

            BuildIndices();         // combined from polygons
            //BuildIndexBuffer();
        }

        public void BuildPolygons()
        {
            Polygons = new List<Model.Polygon>();

            foreach (var polygon in CellStruct.Polygons.Values)
                Polygons.Add(new Model.Polygon(polygon, UVLookup));
        }

        public void BuildVertexBuffer()
        {
            W_VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), W_VertexArray.Count, BufferUsage.WriteOnly);
            W_VertexBuffer.SetData(W_VertexArray.ToArray());

            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), VertexArray.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData(VertexArray.ToArray());
        }

        public void BuildIndices()
        {
            Indices = new List<ushort>();

            foreach (var polygon in Polygons)
                Indices.AddRange(polygon.Indices);
        }

        public void BuildIndexBuffer()
        {
            IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), Indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices.ToArray());
        }

        public static void SetRasterizerState(FillMode fillMode)
        {
            var rs = new RasterizerState();
            //rs.CullMode = CullMode.CullClockwiseFace;
            rs.CullMode = CullMode.None;
            rs.FillMode = fillMode;
            GraphicsDevice.RasterizerState = rs;
        }

        public void Draw(List<Texture2D> textures = null)
        {
            if (W_VertexBuffer == null)
                BuildVertexBuffer();

            var wireframe = textures == null;
            var vertexBuffer = wireframe ? W_VertexBuffer : VertexBuffer;
            var fillMode = wireframe ? FillMode.WireFrame : FillMode.Solid;

            SetRasterizerState(fillMode);

            GraphicsDevice.SetVertexBuffer(vertexBuffer);

            if (wireframe)
            {
                DrawWireframe();
                return;
            }

            foreach (var polygon in Polygons)
            {
                if (polygon.IndexBuffer == null)
                    polygon.BuildIndexBuffer();

                GraphicsDevice.Indices = polygon.IndexBuffer;

                var surfaceIdx = polygon._polygon.PosSurface;
                Effect.Parameters["xTextures"].SetValue(textures[surfaceIdx]);

                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    var indexCnt = polygon.Indices.Count;
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexCnt / 3);
                    DrawCount.NumCellStruct++;
                }
            }
        }

        public void DrawWireframe()
        {
            if (IndexBuffer == null)
                BuildIndexBuffer();

            GraphicsDevice.Indices = IndexBuffer;
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                var indexCnt = Indices.Count;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexCnt / 3);
                DrawCount.NumCellStruct++;
            }
        }
    }
}

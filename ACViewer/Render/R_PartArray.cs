using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

using ACE.Server.Physics;

namespace ACViewer.Render
{
    public class R_PartArray
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public PartArray PartArray { get; set; }
        public List<R_PhysicsPart> Parts { get; set; }

        public VertexPositionColor[] Vertices { get; set; }
        public int[] Indices { get; set; }

        public List<int> VertexOffsets { get; set; }
        public List<int> IndexOffsets { get; set; }

        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }

        public R_PartArray(PartArray partArray)
        {
            PartArray = partArray;
            Parts = new List<R_PhysicsPart>();

            var numVerts = 0;
            var numPolys = 0;

            for (var i = 0; i < partArray.Parts.Count; i++)
            {
                var physicsPart = partArray.Parts[i];

                // skip anchor locations
                if (partArray.Parts[i].GfxObj.ID == 0x010001ec)
                    continue;

                Parts.Add(new R_PhysicsPart(physicsPart));

                numVerts += physicsPart.GfxObj.VertexArray.Vertices.Count;
                numPolys += physicsPart.GfxObj.Polygons.Count;
            }

            //Console.WriteLine("NumParts: " + Parts.Count);
            //Console.WriteLine("NumVerts: " + numVerts);
            //Console.WriteLine("NumPolys: " + numPolys);

            BuildVertices();
            BuildIndices();

            //SetUpBuffers();
        }

        public void BuildVertices()
        {
            var vertices = new List<VertexPositionColor>();

            VertexOffsets = new List<int>();

            foreach (var part in Parts)
            {
                VertexOffsets.Add(vertices.Count);
                vertices.AddRange(part.R_GfxObj.Vertices);
            }

            Vertices = vertices.ToArray();
        }

        public void BuildIndices()
        {
            var indices = new List<int>();

            IndexOffsets = new List<int>();

            foreach (var part in Parts)
            {
                IndexOffsets.Add(indices.Count);
                indices.AddRange(part.R_GfxObj.Indices);
            }
            Indices = indices.ToArray();
        }

        public void SetUpBuffers()
        {
            if (Vertices == null || Vertices.Length == 0)
                return;

            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), Vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(Vertices);

            IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), Indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Server.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Render
{
    public class R_PartArray
    {
        public static GraphicsDevice GraphicsDevice { get => GameView.Instance.GraphicsDevice; }

        public PartArray PartArray;
        public List<R_PhysicsPart> Parts;

        public VertexPositionColor[] Vertices;
        public int[] Indices;

        public List<int> VertexOffsets;
        public List<int> IndexOffsets;

        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;

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
            VertexBuffer.SetData<VertexPositionColor>(Vertices);

            IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), Indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices);
        }
    }
}

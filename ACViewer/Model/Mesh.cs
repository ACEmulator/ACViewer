using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.Server.Physics.Common;

namespace ACViewer.Model
{
    /// <summary>
    /// A 3D mesh of vertices and indices for outdoor landblock terrain
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// The list of vertices comprising the mesh
        /// </summary>
        public List<VertexPositionColor> Vertices;

        /// <summary>
        /// A list of indices into the vertex array
        /// </summary>
        public List<short> Indices;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Mesh() { }

        /// <summary>
        /// Loads the vertices for a landblock mesh
        /// </summary>
        /// <param name="height">The height of each vertex in the landblock cells</param>
        public void LoadVertices(float[,] height)
        {
            var xSize = height.GetLength(0);
            var ySize = height.GetLength(1);

            Vertices = new List<VertexPositionColor>(xSize * ySize);

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    Vertices.Add(new VertexPositionColor(new Vector3(x * LandDefs.CellLength, y * LandDefs.CellLength, height[x, y]), Color.White));
                }
            }
        }

        public void BuildIndices(Landblock landblock)
        {
            var cellDim = LandDefs.BlockSide;
            var vertexDim = LandDefs.VertexDim;

            Indices = new List<short>();

            for (int x = 0; x < cellDim; x++)
            {
                for (int y = 0; y < cellDim; y++)
                {
                    var lowerLeft = (short)(x + y * vertexDim);
                    var lowerRight = (short)((x + 1) + y * vertexDim);
                    var topLeft = (short)(x + (y + 1) * vertexDim);
                    var topRight = (short)((x + 1) + (y + 1) * vertexDim);

                    // determine where to draw the split line
                    if (GetSplitDir(landblock, x, y))
                    {
                        // clockwise winding order
                        Indices.AddRange(new List<short>() { topLeft, lowerRight, lowerLeft });
                        Indices.AddRange(new List<short>() { topLeft, topRight, lowerRight });
                    }
                    else
                    {
                        Indices.AddRange(new List<short>() { topRight, lowerRight, lowerLeft });
                        Indices.AddRange(new List<short>() { topRight, lowerLeft, topLeft });
                    }
                }
            }

        }

        /// <summary>
        /// Determines the split line direction
        /// for a cell triangulation
        /// </summary>
        /// <param name="id">A reference to the landblock ID</param>
        /// <param name="cellX">The horizontal cell position within the landblock</param>
        /// <param name="cellY">The vertical cell position within the landblock</param>
        /// <returns>TRUE if NW-SE split, FALSE if NE-SW split</returns>
        public bool GetSplitDir(Landblock landblock, int cellX, int cellY)
        {
            var lbx = landblock.ID >> 24;
            var lby = landblock.ID >> 16 & 0xFF;
            
            // get the global tile offsets
            var x = (lbx * 8) + cellX;
            var y = (lby * 8) + cellY;

            // Thanks to https://github.com/deregtd/AC2D for this bit
            var dw = x * y * 0x0CCAC033 - x * 0x421BE3BD + y * 0x6C1AC587 - 0x519B8F25;
            return (dw & 0x80000000) == 0;
        }
    }
}

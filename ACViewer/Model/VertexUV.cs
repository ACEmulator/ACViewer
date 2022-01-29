using System;

namespace ACViewer.Model
{
    public class VertexUV: IEquatable<VertexUV>
    {
        public int VertexIdx { get; set; }
        public int UVIdx { get; set;  }

        public VertexUV(int vertexIdx, int uvIdx)
        {
            VertexIdx = vertexIdx;
            UVIdx = uvIdx;
        }

        public bool Equals(VertexUV v)
        {
            return VertexIdx == v.VertexIdx && UVIdx == v.UVIdx;
        }

        public override int GetHashCode()
        {
            int hash = 0;

            hash = (hash * 397) ^ VertexIdx.GetHashCode();
            hash = (hash * 397) ^ UVIdx.GetHashCode();

            return hash;
        }
    }
}

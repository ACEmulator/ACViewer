using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ACE.DatLoader.Entity;

namespace ACViewer
{ 
    public static class VertexArrayExtensions
    {
        public static List<VertexPositionNormalTexture> ToXna(this CVertexArray vertexArray)
        {
            var verts = new List<VertexPositionNormalTexture>();

            foreach (var v in vertexArray.Vertices)
                verts.AddRange(v.Value.ToXna());

            return verts;
        }

        public static List<VertexPositionColor> ToWireframeXna(this CVertexArray vertexArray)
        {
            var verts = new List<VertexPositionColor>();

            foreach (var v in vertexArray.Vertices)
                verts.AddRange(v.Value.ToWireframeXna());

            return verts;
        }

        public static Dictionary<Tuple<ushort, ushort>, ushort> BuildUVLookup(this CVertexArray vertexArray)
        {
            var uvLookupTable = new Dictionary<Tuple<ushort, ushort>, ushort>();

            ushort i = 0;
            foreach (var v in vertexArray.Vertices)
            {
                if (v.Value.UVs == null || v.Value.UVs.Count == 0)
                {
                    uvLookupTable.Add(new Tuple<ushort, ushort>(v.Key, 0), i++);
                    continue;
                }

                for (ushort uvIdx = 0; uvIdx < v.Value.UVs.Count; uvIdx++)
                    uvLookupTable.Add(new Tuple<ushort, ushort>(v.Key, uvIdx), i++);
            }
            return uvLookupTable;
        }

        public static bool Verify(this CVertexArray vertexArray)
        {
            var keys = vertexArray.Vertices.Keys.ToList();
            keys.Sort();

            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (key != i)
                    return false;
            }
            return true;
        }
    }
}

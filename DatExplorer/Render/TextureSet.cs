using System;
using System.Collections.Generic;

namespace DatExplorer.Render
{
    public class TextureSet: IEquatable<TextureSet>
    {
        public List<uint> SurfaceIDs;

        public TextureSet(R_EnvCell envCell)
        {
            SurfaceIDs = envCell.EnvCell._envCell.Surfaces;
        }

        public bool Equals(TextureSet textureSet)
        {
            if (SurfaceIDs.Count != textureSet.SurfaceIDs.Count)
                return false;

            for (var i = 0; i < SurfaceIDs.Count; i++)
            {
                if (SurfaceIDs[i] != textureSet.SurfaceIDs[i])
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;

            foreach (var surfaceID in SurfaceIDs)
                hash = (hash * 397) ^ surfaceID.GetHashCode();

            return hash;
        }
    }
}

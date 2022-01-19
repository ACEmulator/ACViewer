using System;
using System.Collections.Generic;

namespace ACViewer.Render
{
    public class TextureSet: IEquatable<TextureSet>
    {
        public uint Environment { get; set; }
        public List<uint> SurfaceIDs { get; set; }

        public TextureSet(R_EnvCell envCell)
        {
            Environment = envCell.EnvCell.EnvironmentID;
            SurfaceIDs = envCell.EnvCell._envCell.Surfaces;
        }

        public bool Equals(TextureSet textureSet)
        {
            if (Environment != textureSet.Environment)
                return false;

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

            hash = (hash * 397) ^ Environment.GetHashCode();

            foreach (var surfaceID in SurfaceIDs)
                hash = (hash * 397) ^ surfaceID.GetHashCode();

            return hash;
        }
    }
}

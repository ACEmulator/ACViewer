﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DatExplorer.Render
{
    public class EffectParameters: IEquatable<EffectParameters>
    {
        //public string Technique;
        
        // per-draw method
        //public Matrix World;
        //public Matrix View;
        //public Matrix Projection;

        // per-draw call
        public Texture2D Texture;

        // texture arrays for landscape
        public Texture2D Overlays;
        public Texture2D Alphas;

        public bool Equals(EffectParameters effectParameters)
        {
            return //Technique.Equals(Technique) &&
                   Texture == effectParameters.Texture &&
                   Overlays == effectParameters.Overlays &&
                   Alphas == effectParameters.Alphas;
        }

        public override int GetHashCode()
        {
            int hash = 0;

            //hash = (hash * 397) ^ Technique.GetHashCode();

            if (Texture != null)
                hash = (hash * 397) ^ Texture.GetHashCode();

            if (Overlays != null)
                hash = (hash * 397) ^ Overlays.GetHashCode();

            if (Alphas != null)
                hash = (hash * 397) ^ Alphas.GetHashCode();

            return hash;
        }

        public EffectParameters GlobalClone()
        {
            var clone = new EffectParameters();

            //clone.World = World;
            //clone.View = View;
            //clone.Projection = Projection;

            return clone;
        }

        public EffectParameters LocalClone()
        {
            var clone = new EffectParameters();

            //clone.Technique = Technique;
            clone.Texture = Texture;
            clone.Overlays = Overlays;
            clone.Alphas = Alphas;

            return clone;
        }

        public void Dispose()
        {
            if (Texture != null)
                Texture.Dispose();
            if (Overlays != null)
                Overlays.Dispose();
            if (Alphas != null)
                Alphas.Dispose();
        }
    }
}

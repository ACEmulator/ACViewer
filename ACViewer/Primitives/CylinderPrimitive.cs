#region File Description
//-----------------------------------------------------------------------------
// SpherePrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace ACViewer.Primitives
{
    /// <summary>
    /// Geometric primitive class for drawing spheres.
    /// 
    /// This class is borrowed from the Primitives3D sample.
    /// </summary>
    public class CylinderPrimitive : GeometricPrimitive
    {
        /// <summary>
        /// Constructs a new sphere primitive, using default settings.
        /// </summary>
        public CylinderPrimitive(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, 1, 1, 16)
        {
        }


        /// <summary>
        /// Constructs a new sphere primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public CylinderPrimitive(GraphicsDevice graphicsDevice, float diameter, float height, int tessellation)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            float radius = diameter / 2;

            var stepSize = (float)Math.PI * 2 / tessellation;

            // create vertices
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < tessellation; j++)
                {
                    var q = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, j * stepSize);
                    var local = Vector3.Transform(Vector3.UnitY * radius, q);

                    if (i == 1) local.Z = height;

                    AddVertex(local, local);
                }
            }

            // create indices
            for (var i = 0; i < tessellation; i++)
            {
                var next = i < tessellation - 1 ? i + 1 : 0;
                
                AddIndex(i);
                AddIndex(next);
                AddIndex(i + tessellation);

                AddIndex(next);
                AddIndex(i + tessellation);
                AddIndex(next + tessellation);
            }

            InitializePrimitive(graphicsDevice);
        }
    }
}
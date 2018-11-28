﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using DatExplorer.Render;
using DatExplorer.View;

namespace DatExplorer.Model
{
    public class GfxObj
    {
        public static GraphicsDevice GraphicsDevice { get => GameView.Instance.GraphicsDevice; }

        public ACE.DatLoader.FileTypes.GfxObj _gfxObj;

        public List<VertexPositionNormalTexture> VertexArray;
        public Dictionary<Tuple<ushort, ushort>, ushort> UVLookup;

        public List<Polygon> Polygons;

        public VertexBuffer VertexBuffer;

        public List<Texture2D> Textures;

        public List<Surface> Surfaces;

        public BoundingBox BoundingBox;

        public GfxObj(uint gfxObjID)
        {
            MainWindow.Instance.Status.WriteLine($"Loading GfxObj {gfxObjID:X8}");

            _gfxObj = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.GfxObj>(gfxObjID);

            VertexArray = _gfxObj.VertexArray.ToXna();
            UVLookup = _gfxObj.VertexArray.BuildUVLookup();

            if (!_gfxObj.VertexArray.Verify())
                Console.WriteLine($"ERROR: vertex array {gfxObjID:X8}");

            LoadTextures();

            BuildPolygons();

            //BuildVertexBuffer();
        }

        public void BuildPolygons()
        {
            Polygons = new List<Polygon>();

            foreach (var polygon in _gfxObj.Polygons.Values)
                Polygons.Add(new Polygon(this, polygon));
        }

        public void BuildVertexBuffer()
        {
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), VertexArray.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData(VertexArray.ToArray());
        }

        public void LoadTextures()
        {
            Textures = new List<Texture2D>();
            Surfaces = new List<Surface>();

            foreach (var surfaceID in _gfxObj.Surfaces)
            {
                var surface = DatManager.PortalDat.ReadFromDat<Surface>(surfaceID);
                Surfaces.Add(surface);

                Textures.Add(TextureCache.Get(surfaceID));
            }
        }
    }
}

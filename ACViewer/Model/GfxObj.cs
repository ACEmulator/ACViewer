﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

using ACViewer.Render;
using ACViewer.View;

namespace ACViewer.Model
{
    public class GfxObj
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public ACE.DatLoader.FileTypes.GfxObj _gfxObj { get; set; }

        public List<VertexPositionNormalTexture> VertexArray { get; set; }
        public Dictionary<Tuple<ushort, ushort>, ushort> UVLookup { get; set; }

        public List<Polygon> Polygons { get; set; }

        public VertexBuffer VertexBuffer { get; set; }

        public List<Texture2D> Textures { get; set; }

        public List<Surface> Surfaces { get; set; }

        public BoundingBox BoundingBox { get; set; }

        private bool? hasWrappingUVs;
        
        public bool HasWrappingUVs
        {
            get
            {
                if (hasWrappingUVs == null)
                    hasWrappingUVs = _gfxObj.VertexArray.HasWrappingUVs();

                return hasWrappingUVs.Value;
            }
        }

        public GfxObj(uint gfxObjID, bool doBuild = true)
        {
            MainWindow.Instance.Status.WriteLine($"Loading GfxObj {gfxObjID:X8}");

            _gfxObj = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.GfxObj>(gfxObjID);

            VertexArray = _gfxObj.VertexArray.ToXna();
            UVLookup = _gfxObj.VertexArray.BuildUVLookup();

            if (!_gfxObj.VertexArray.Verify())
                Console.WriteLine($"ERROR: vertex array {gfxObjID:X8}");

            // If we are doing any texture/palette swaps, we'll do this after we swap them
            if (doBuild)
            {
                LoadTextures();

                BuildPolygons();
            }

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
            // bad data 02001C50
            if (VertexArray.Count == 0) return;
            
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), VertexArray.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData(VertexArray.ToArray());
        }

        public void LoadTextures(List<ACE.DatLoader.Entity.TextureMapChange> textureChanges = null, Dictionary<int, uint> customPaletteColors = null, bool useCache = true)
        {
            Textures = new List<Texture2D>();
            Surfaces = new List<Surface>();

            foreach (var surfaceID in _gfxObj.Surfaces)
            {
                var surface = DatManager.PortalDat.ReadFromDat<Surface>(surfaceID);
                Surfaces.Add(surface);

                Textures.Add(TextureCache.Get(surfaceID, textureChanges, customPaletteColors, useCache));
            }
        }

        public List<Vector3> GetVertices()
        {
            return VertexArray.Select(v => v.Position).ToList();
        }

        public void BuildBoundingBox()
        {
            var verts = GetVertices();
            BoundingBox = new BoundingBox(verts);
        }
    }
}

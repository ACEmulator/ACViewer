using System;
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

                if (surface.ColorValue != 0)
                {
                    // swatch
                    var swatch = new Texture2D(GraphicsDevice, 1, 1);
                    var a = surface.ColorValue >> 24;
                    var r = (surface.ColorValue >> 16) & 0xFF;
                    var g = (surface.ColorValue >> 8) & 0xFF;
                    var b = surface.ColorValue & 0xFF;
                    a = 0;
                    swatch.SetData(new Color[] { new Color(r, g, b, a) });
                    Textures.Add(swatch);
                    continue;
                }

                var surfaceTexture = DatManager.PortalDat.ReadFromDat<SurfaceTexture>(surface.OrigTextureId);

                foreach (var textureID in surfaceTexture.Textures)
                {
                    Textures.Add(TextureCache.LoadTexture(surface, textureID));
                    break;
                }
            }
        }
    }
}

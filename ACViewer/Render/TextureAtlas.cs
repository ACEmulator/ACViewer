using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

using ACViewer.Model;

namespace ACViewer.Render
{
    public class TextureAtlas
    {
        public GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public TextureFormat TextureFormat { get; set; }

        public Dictionary<SurfaceTexturePalette, int> Textures { get; set; }    // surface id => texture idx

        public Texture2D _Textures { get; set; }

        public TextureAtlas(TextureFormat textureFormat)
        {
            TextureFormat = textureFormat;

            Textures = new Dictionary<SurfaceTexturePalette, int>();
        }

        public int GetTextureIdx(uint surfaceID, Dictionary<uint, uint> textureChanges = null, PaletteChanges paletteChanges = null)
        {
            var surfaceTextureId = TextureCache.GetSurfaceTextureID(surfaceID, textureChanges);

            var surfaceTexturePalette = new SurfaceTexturePalette(surfaceID, surfaceTextureId, paletteChanges);
            
            if (!Textures.TryGetValue(surfaceTexturePalette, out var idx))
            {
                idx = Textures.Count;
                Textures.Add(surfaceTexturePalette, idx);
            }
            return idx;
        }

        public void OnCompleted()
        {
            BuildTextures();
        }

        private void BuildTextures()
        {
            if (Textures.Count == 0) return;

            var useMipMaps = false;

            if (TextureCache.UseMipMaps)
            {
                // pre-fetch first texture to determine if we can use mipmaps
                var stp = Textures.First().Key;
                var texture = TextureCache.Get(stp.OrigSurfaceId, stp.SurfaceTextureId, stp.PaletteChanges);
                if (texture.LevelCount > 1)
                    useMipMaps = true;
            }

            // max size / # of textures?
            _Textures = new Texture2D(GraphicsDevice, TextureFormat.Width, TextureFormat.Height, useMipMaps, TextureFormat.SurfaceFormat, Textures.Count);

            foreach (var kvp in Textures)
            {
                var stp = kvp.Key;
                var textureIdx = kvp.Value;

                //Console.WriteLine($"Adding {textureID:X8}");

                var texture = TextureCache.Get(stp.OrigSurfaceId, stp.SurfaceTextureId, stp.PaletteChanges);
                var numLevels = texture.LevelCount;
                var numColors = texture.Width * texture.Height;

                if (texture.Format == SurfaceFormat.Alpha8)
                {
                    var alphaData = new byte[numColors];
                    texture.GetData(alphaData, 0, numColors);

                    _Textures.SetData(0, textureIdx, null, alphaData, 0, numColors);
                }
                else
                {
                    if (texture.Format == SurfaceFormat.Dxt1)
                        numColors /= 8;
                    else if (texture.Format == SurfaceFormat.Dxt3 || texture.Format == SurfaceFormat.Dxt5)
                        numColors /= 4;

                    var mipData = texture.GetMipData(numColors);

                    for (var i = 0; i < numLevels; i++)
                        _Textures.SetData(i, textureIdx, null, mipData[i], 0, mipData[i].Length);
                }
            }
        }

        public void Dispose()
        {
            if (_Textures != null)
                _Textures.Dispose();
        }
    }
}

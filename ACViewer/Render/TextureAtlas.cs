using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Render
{
    public class TextureAtlas
    {
        public GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public TextureFormatChain TextureFormatChain { get; set; }

        public Dictionary<SurfaceTexturePalette, int> Textures { get; set; }    // surface id => texture idx

        public Texture2D _Textures { get; set; }

        public TextureAtlas(TextureFormat textureFormat, int atlasIdx)
        {
            TextureFormatChain = new TextureFormatChain(textureFormat, atlasIdx);

            Textures = new Dictionary<SurfaceTexturePalette, int>();
        }

        public void OnCompleted()
        {
            BuildTextures();
        }

        private void BuildTextures()
        {
            if (Textures.Count == 0) return;

            if (_Textures != null)
                _Textures.Dispose();

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
            // Textures.Count max 2048, regardless of width/height/surfaceformat/mipmaps?
            var textureFormat = TextureFormatChain.TextureFormat;
            _Textures = new Texture2D(GraphicsDevice, textureFormat.Width, textureFormat.Height, useMipMaps, textureFormat.SurfaceFormat, Textures.Count);

            var firstIdx = -1;
            
            foreach (var kvp in Textures)
            {
                var stp = kvp.Key;
                var textureIdx = kvp.Value;

                if (firstIdx == -1)
                    firstIdx = textureIdx;

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
                        _Textures.SetData(i, textureIdx - firstIdx, null, mipData[i], 0, mipData[i].Length);
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

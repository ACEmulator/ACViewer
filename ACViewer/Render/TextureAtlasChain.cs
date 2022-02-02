using System.Collections.Generic;

namespace ACViewer.Render
{
    // Textures.Count max 2048 in each TextureAtlas, regardless of width/height/surface format/mipmaps
    public class TextureAtlasChain
    {
        public TextureFormat TextureFormat { get; set; }

        public Dictionary<SurfaceTexturePalette, int> TextureAtlasIndices { get; set; }    // surface id => texture atlas idx

        public List<TextureAtlas> TextureAtlases { get; set; }

        public TextureAtlas CurrentTextureAtlas { get; set; }

        public TextureAtlasChain(TextureFormat textureFormat)
        {
            TextureFormat = textureFormat;

            TextureAtlasIndices = new Dictionary<SurfaceTexturePalette, int>();

            TextureAtlases = new List<TextureAtlas>();
        }

        private static readonly int MaxTexturesPerAtlas = 2048;

        public int GetAtlasIdx(SurfaceTexturePalette surfaceTexturePalette)
        {
            if (!TextureAtlasIndices.TryGetValue(surfaceTexturePalette, out var atlasIdx))
            {
                if (CurrentTextureAtlas == null || CurrentTextureAtlas.Textures.Count >= MaxTexturesPerAtlas)
                {
                    CurrentTextureAtlas = new TextureAtlas(TextureFormat, TextureAtlases.Count);
                    TextureAtlases.Add(CurrentTextureAtlas);
                }
                atlasIdx = CurrentTextureAtlas.TextureFormatChain.AtlasChainIdx;

                CurrentTextureAtlas.Textures.Add(surfaceTexturePalette, TextureAtlasIndices.Count);
                TextureAtlasIndices.Add(surfaceTexturePalette, atlasIdx);
            }

            return atlasIdx;
        }

        public int GetTextureIdx(uint textureId)
        {
            var stp = new SurfaceTexturePalette(textureId, textureId);

            var atlasIdx = GetAtlasIdx(stp);

            return TextureAtlases[atlasIdx].Textures[stp];
        }

        public void OnCompleted()
        {
            foreach (var textureAtlas in TextureAtlases)
                textureAtlas.OnCompleted();
        }

        public void Dispose()
        {
            foreach (var textureAtlas in TextureAtlases)
                textureAtlas.Dispose();
        }
    }
}

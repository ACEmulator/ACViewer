using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace ACViewer
{
    public static class TextureExtensions
    {
        public static List<byte[]> GetMipData(this Texture2D texture, int size)
        {
            var numLevels = texture.LevelCount;

            var mipData = new List<byte[]>();

            var mipsize = size * 4;
            for (var i = 0; i < numLevels; i++)
            {
                var data = new byte[mipsize];
                texture.GetData(i, null, data, 0, mipsize);
                mipData.Add(data);
                mipsize /= 4;
            }
            return mipData;
        }
    }
}

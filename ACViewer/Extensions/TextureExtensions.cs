using System;
using System.Collections.Generic;
using System.Windows;

using Microsoft.Xna.Framework;
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

        public static void SetDataAsync(this Texture2D texture, byte[] data)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                texture.SetData(data);
            }));
        }

        public static void SetDataAsync(this Texture2D texture, Color[] data)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                texture.SetData(data);
            }));
        }

        public static void SetDataAsync(this Texture2D texture, int level, Rectangle? rect, byte[] data, int startIndex, int elementCount)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                texture.SetData(level, rect, data, startIndex, elementCount);
            }));
        }
    }
}

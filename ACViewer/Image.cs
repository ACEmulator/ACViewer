using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer
{
    public static class Image
    {
        public static List<Microsoft.Xna.Framework.Color> GetColors(Bitmap bitmap)
        {
            var numColors = bitmap.Width * bitmap.Height;
            var colors = new List<Microsoft.Xna.Framework.Color>();

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);

                    var c = new Microsoft.Xna.Framework.Color();
                    c.R = pixel.R;
                    c.G = pixel.G;
                    c.B = pixel.B;
                    c.A = pixel.A;

                    colors.Add(c);
                }
            }
            return colors;
        }

        public static Texture2D GetTextureFromBitmap(GraphicsDevice device, Bitmap bitmap)
        {
            var colors = GetColors(bitmap).ToArray();
            var texture = new Texture2D(device, bitmap.Width, bitmap.Height, false, SurfaceFormat.Color);
            texture.SetData(colors);
            return texture;
        }

        public static Texture2D GetTexture2DFromBitmap(GraphicsDevice device, Bitmap bitmap)
        {
            Texture2D tex = new Texture2D(device, bitmap.Width, bitmap.Height, false, SurfaceFormat.Color);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int bufferSize = data.Height * data.Stride;

            //create data buffer 
            byte[] bytes = new byte[bufferSize];

            // copy bitmap data into buffer
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            // copy our buffer to the texture
            tex.SetData(bytes);

            // unlock the bitmap data
            bitmap.UnlockBits(data);

            return tex;
        }
    }
}

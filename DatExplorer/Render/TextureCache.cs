using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using DatExplorer.Model;
using DatExplorer.View;

namespace DatExplorer.Render
{
    public class TextureCache
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;
        public static SpriteBatch spriteBatch => GameView.Instance.SpriteBatch;

        public static Dictionary<uint, Texture2D> Textures;

        static TextureCache()
        {
            Init();
        }

        public static void Init(bool dispose = true)
        {
            if (dispose && Textures != null)
                foreach (var texture in Textures.Values)
                    texture.Dispose();

            GfxObjCache.Init();
            SetupCache.Init();

            Textures = new Dictionary<uint, Texture2D>();
        }

        private static Texture2D LoadTexture(uint textureID, bool useDummy = false)
        {
            if (textureID >> 24 == 0x04)
                return LoadPalette(textureID);
            else if (textureID >> 24 == 0x0F)
                return LoadPaletteSet(textureID);

            MainWindow.Instance.Status.WriteLine($"Loading texture {textureID:X8}");

            var texture = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Texture>(textureID);
            if (texture.SourceData == null)
                texture = DatManager.HighResDat.ReadFromDat<ACE.DatLoader.FileTypes.Texture>(textureID);

            var surfaceFormat = SurfaceFormat.Color;
            switch (texture.Format)
            {
                case SurfacePixelFormat.PFID_DXT1:
                    surfaceFormat = SurfaceFormat.Dxt1;
                    break;
                case SurfacePixelFormat.PFID_DXT3:
                    surfaceFormat = SurfaceFormat.Dxt3;
                    break;
                case SurfacePixelFormat.PFID_DXT5:
                    //if (!isClipMap)
                    surfaceFormat = SurfaceFormat.Dxt5;
                    break;
                case SurfacePixelFormat.PFID_CUSTOM_LSCAPE_ALPHA:
                case SurfacePixelFormat.PFID_A8:
                case SurfacePixelFormat.PFID_P8:    // indexed color
                    surfaceFormat = SurfaceFormat.Alpha8;
                    break;
            }

            var width = texture.Width;
            var height = texture.Height;

            var data = new byte[texture.SourceData.Length];
            Array.Copy(texture.SourceData, data, data.Length);  // fixme: multiple rgb reversals

            if (surfaceFormat == SurfaceFormat.Color)
            {
                switch (texture.Format)
                {
                    case SurfacePixelFormat.PFID_R8G8B8:
                    case SurfacePixelFormat.PFID_CUSTOM_LSCAPE_R8G8B8:
                        data = AddAlpha(data);
                        break;
                    case SurfacePixelFormat.PFID_INDEX16:
                        data = IndexToColor(texture);
                        break;

                    case SurfacePixelFormat.PFID_CUSTOM_RAW_JPEG:
                    case SurfacePixelFormat.PFID_R5G6B5:
                    case SurfacePixelFormat.PFID_A4R4G4B4:
                        //case SurfacePixelFormat.PFID_DXT5:
                        var bitmap = texture.GetBitmap();
                        var _tex = GetTexture2DFromBitmap(GameView.Instance.GraphicsDevice, bitmap);

                        //if (isClipMap)
                        //AdjustClip(_tex);

                        return _tex;

                    case SurfacePixelFormat.PFID_A8R8G8B8:
                        ConvertToABGR(data);
                        break;
                }
            }

            Texture2D tex;
            if (useDummy)
            {
                if (surfaceFormat == SurfaceFormat.Alpha8)
                {
                    tex = new Texture2D(GameView.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Alpha8);
                    var alpha = new byte[1];
                    alpha[0] = 255;
                    tex.SetData(alpha);
                }
                else
                {
                    tex = new Texture2D(GameView.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                    var color = new Microsoft.Xna.Framework.Color[1];
                    color[0].A = data[3];
                    color[0].R = data[2];
                    color[0].G = data[1];
                    color[0].B = data[0];
                    tex.SetData(color);
                }
            }
            else
            {
                if (surfaceFormat == SurfaceFormat.Color && texture.Width == texture.Height)
                    tex = GenerateMipMaps(data, texture.Width);
                else
                {
                    tex = new Texture2D(GameView.Instance.GraphicsDevice, texture.Width, texture.Height, false, surfaceFormat);
                    tex.SetData(data);
                }
            }

            return tex;
        }

        private static Texture2D LoadTexture(Surface surface, uint textureID, bool useDummy = false, Dictionary<int, uint> customPaletteColors = null)
        {
            MainWindow.Instance.Status.WriteLine($"Loading texture {textureID:X8}");

            var isClipMap = surface.Type.HasFlag(SurfaceType.Base1ClipMap);

            var texture = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Texture>(textureID);
            if (texture.SourceData == null)
                texture = DatManager.HighResDat.ReadFromDat<ACE.DatLoader.FileTypes.Texture>(textureID);

            if (customPaletteColors != null)
                texture.CustomPaletteColors = customPaletteColors;

            var surfaceFormat = SurfaceFormat.Color;
            switch (texture.Format)
            {
                case SurfacePixelFormat.PFID_DXT1:
                    surfaceFormat = SurfaceFormat.Dxt1;
                    break;
                case SurfacePixelFormat.PFID_DXT3:
                    surfaceFormat = SurfaceFormat.Dxt3;
                    break;
                case SurfacePixelFormat.PFID_DXT5:
                    //if (!isClipMap)
                    surfaceFormat = SurfaceFormat.Dxt5;
                    break;
                case SurfacePixelFormat.PFID_CUSTOM_LSCAPE_ALPHA:
                case SurfacePixelFormat.PFID_A8:
                case SurfacePixelFormat.PFID_P8:    // indexed color
                    surfaceFormat = SurfaceFormat.Alpha8;
                    break;
            }

            var width = texture.Width;
            var height = texture.Height;

            var data = new byte[texture.SourceData.Length];
            Array.Copy(texture.SourceData, data, data.Length);  // fixme: multiple rgb reversals

            if (surfaceFormat == SurfaceFormat.Color)
            {
                switch (texture.Format)
                {
                    case SurfacePixelFormat.PFID_R8G8B8:
                    case SurfacePixelFormat.PFID_CUSTOM_LSCAPE_R8G8B8:
                        data = AddAlpha(data);
                        break;
                    case SurfacePixelFormat.PFID_INDEX16:
                        data = IndexToColor(texture, isClipMap);
                        break;

                    case SurfacePixelFormat.PFID_CUSTOM_RAW_JPEG:
                    case SurfacePixelFormat.PFID_R5G6B5:
                    case SurfacePixelFormat.PFID_A4R4G4B4:
                        //case SurfacePixelFormat.PFID_DXT5:
                        var bitmap = texture.GetBitmap();
                        var _tex = GetTexture2DFromBitmap(GameView.Instance.GraphicsDevice, bitmap);

                        //if (isClipMap)
                        //AdjustClip(_tex);

                        return _tex;

                    case SurfacePixelFormat.PFID_A8R8G8B8:
                        ConvertToABGR(data);
                        break;
                }
            }

            Texture2D tex;
            if (useDummy)
            {
                if (surfaceFormat == SurfaceFormat.Alpha8)
                {
                    tex = new Texture2D(GameView.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Alpha8);
                    var alpha = new byte[1];
                    alpha[0] = 255;
                    tex.SetData(alpha);
                }
                else
                {
                    tex = new Texture2D(GameView.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                    var color = new Microsoft.Xna.Framework.Color[1];
                    color[0].A = data[3];
                    color[0].R = data[2];
                    color[0].G = data[1];
                    color[0].B = data[0];
                    tex.SetData(color);
                }
            }
            else
            {
                if (surfaceFormat == SurfaceFormat.Color && texture.Width == texture.Height)
                    tex = GenerateMipMaps(data, texture.Width);
                else
                {
                    tex = new Texture2D(GameView.Instance.GraphicsDevice, texture.Width, texture.Height, false, surfaceFormat);
                    tex.SetData(data);
                }
            }

            return tex;
        }

        private static void AdjustClip(Texture2D texture)
        {
            var colors = new Microsoft.Xna.Framework.Color[texture.Width * texture.Height];
            texture.GetData(colors);
            for (var i = 0; i < colors.Length; i++)
            {
                if (colors[i].A < 8)
                    colors[i] = new Microsoft.Xna.Framework.Color(0, 0, 0, 0);
            }
            texture.SetData(colors);
        }

        private static SurfaceFormat GetSurfaceFormat(SurfacePixelFormat spf)
        {
            switch (spf)
            {
                case SurfacePixelFormat.PFID_INDEX16:
                    return SurfaceFormat.Color;

                case SurfacePixelFormat.PFID_A8:
                case SurfacePixelFormat.PFID_L8:
                    return SurfaceFormat.Alpha8;

                case SurfacePixelFormat.PFID_DXT1:
                    return SurfaceFormat.Dxt1;

                case SurfacePixelFormat.PFID_DXT3:
                    return SurfaceFormat.Dxt3;

                case SurfacePixelFormat.PFID_DXT5:
                    return SurfaceFormat.Dxt5;

                default:
                    return SurfaceFormat.Color;
            }
        }

        private static Texture2D LoadPalette(uint paletteID)
        {
            var colors = LoadPaletteColors(paletteID, out var width, out var height);

            var texture = new Texture2D(GameView.Instance.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            texture.SetData(colors);

            return texture;
        }

        private static List<Microsoft.Xna.Framework.Color> Padding = Enumerable.Repeat(Microsoft.Xna.Framework.Color.Black, 384).ToList();

        private static Texture2D LoadPaletteSet(uint paletteSetID)
        {
            var paletteSet = DatManager.PortalDat.ReadFromDat<PaletteSet>(paletteSetID);

            var numPalettes = paletteSet.PaletteList.Count;

            var colors = new List<Microsoft.Xna.Framework.Color>();
            for (var i = 0; i < paletteSet.PaletteList.Count; i++)
            {
                var paletteID = paletteSet.PaletteList[i];

                colors.AddRange(LoadPaletteColors(paletteID, out var width, out var height).ToList());
                if (i < paletteSet.PaletteList.Count - 1)
                    colors.AddRange(Padding);
            }

            var setWidth = 64;
            var setHeight = colors.Count / setWidth;

            var texture = new Texture2D(GameView.Instance.GraphicsDevice, setWidth, setHeight, false, SurfaceFormat.Color);
            texture.SetData(colors.ToArray());
            return texture;
        }

        private static int PaletteWidth = 64;

        private static Microsoft.Xna.Framework.Color[] LoadPaletteColors(uint paletteID, out int width, out int height)
        {
            var palette = DatManager.PortalDat.ReadFromDat<Palette>(paletteID);

            var numColors = palette.Colors.Count;

            width = Math.Min(numColors, PaletteWidth);
            height = (int)Math.Ceiling((float)numColors / PaletteWidth);

            var colors = new Microsoft.Xna.Framework.Color[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var colorNum = x + y * PaletteWidth;

                    if (colorNum >= numColors)
                    {
                        colors[colorNum] = new Microsoft.Xna.Framework.Color(0, 0, 0, 0);
                        continue;
                    }

                    var c = palette.Colors[colorNum];

                    var color = new Microsoft.Xna.Framework.Color();
                    color.A = (byte)(c >> 24);
                    color.R = (byte)(c >> 16 & 0xFF);
                    color.G = (byte)(c >> 8 & 0xFF);
                    color.B = (byte)(c & 0xFF);

                    colors[colorNum] = color;
                }
            }
            return colors;
        }

        private static Texture2D GetTexture2DFromBitmap(GraphicsDevice device, Bitmap bitmap)
        {
            Texture2D tex = new Texture2D(device, bitmap.Width, bitmap.Height, false, SurfaceFormat.Color);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

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

        private static byte[] AddAlpha(byte[] rgb)
        {
            var rgba = new byte[rgb.Length + rgb.Length / 3];

            var idx = 0;
            for (var i = 0; i < rgb.Length; i += 3)
            {
                rgba[idx++] = rgb[i + 2];
                rgba[idx++] = rgb[i + 1];
                rgba[idx++] = rgb[i];
                rgba[idx++] = 255;
            }
            return rgba;
        }

        private static void ConvertToABGR(byte[] argb)
        {
            for (var i = 0; i < argb.Length; i += 4)
            {
                var tmp = argb[i];
                argb[i] = argb[i + 2];
                argb[i + 2] = tmp;
            }
        }

        private static byte[] IndexToColor(ACE.DatLoader.FileTypes.Texture texture, bool isClipMap = false)
        {
            var colors = GetColors(texture);
            var palette = DatManager.PortalDat.ReadFromDat<Palette>((uint)texture.DefaultPaletteId);

            // Apply any custom palette colors, if any, to our loaded palette (note, this may be all of them!)
            if (texture.CustomPaletteColors.Count > 0)
                foreach (var entry in texture.CustomPaletteColors)
                    if (entry.Key <= palette.Colors.Count)
                        palette.Colors[entry.Key] = entry.Value;

            var output = new byte[texture.Width * texture.Height * 4];

            for (int i = 0; i < texture.Height; i++)
                for (int j = 0; j < texture.Width; j++)
                {
                    int idx = (i * texture.Width) + j;
                    var color = colors[idx];
                    var paletteColor = palette.Colors[color];

                    byte a = Convert.ToByte((paletteColor & 0xFF000000) >> 24);
                    byte r = Convert.ToByte((paletteColor & 0xFF0000) >> 16);
                    byte g = Convert.ToByte((paletteColor & 0xFF00) >> 8);
                    byte b = Convert.ToByte(paletteColor & 0xFF);

                    if (isClipMap && color < 8)
                        r = g = b = a = 0;

                    output[idx * 4] = r;
                    output[idx * 4 + 1] = g;
                    output[idx * 4 + 2] = b;
                    output[idx * 4 + 3] = a;
                }

            return output;
        }

        private static List<int> GetColors(ACE.DatLoader.FileTypes.Texture texture)
        {
            var colors = new List<int>();
            using (BinaryReader reader = new BinaryReader(new MemoryStream(texture.SourceData)))
            {
                for (uint y = 0; y < texture.Height; y++)
                    for (uint x = 0; x < texture.Width; x++)
                        colors.Add(reader.ReadInt16());
            }
            return colors;
        }

        // 0x08 - Surface - contains a 0x05 SurfaceTexture, along with additional type info (clipmask)
        // 0x05 - SurfaceTexture - contains a list of 0x06 textures
        // 0x06 - Texture - image format and data

        public static Texture2D Get(uint fileID, List<ACE.DatLoader.Entity.TextureMapChange> textureMapChanges = null, Dictionary<int, uint> customPaletteColors = null, bool useCache = true)
        {
            if (fileID >> 24 == 0x01)
            {
                // gfxobj
                var gfxObj = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.GfxObj>(fileID);

                var surfaceID = gfxObj.Surfaces[0];
                Surface surface = DatManager.PortalDat.ReadFromDat<Surface>(surfaceID);

                if (surface.ColorValue != 0)
                {
                    // swatch
                    var swatch = new Texture2D(GameView.Instance.GraphicsDevice, 1, 1);
                    var a = surface.ColorValue >> 24;
                    var r = (surface.ColorValue >> 16) & 0xFF;
                    var g = (surface.ColorValue >> 8) & 0xFF;
                    var b = surface.ColorValue & 0xFF;
                    swatch.SetData(new Microsoft.Xna.Framework.Color[] { new Microsoft.Xna.Framework.Color(r, g, b, a) });
                    return swatch;
                }

                uint textureId = surface.OrigTextureId;
                if (textureMapChanges != null)
                {
                    var tmChange = textureMapChanges.Where(t => t.OldTexture == surface.OrigTextureId).FirstOrDefault();
                    if (tmChange != null)
                        textureId = tmChange.NewTexture;
                }
                
                var surfaceTexture = DatManager.PortalDat.ReadFromDat<SurfaceTexture>(textureId);

                return GetTexture(surface, surfaceTexture.Textures[0]);
            }
            else if (fileID >> 24 == 0x04)
            {
                // palette
                return GetTexture(fileID);
            }
            else if (fileID >> 24 == 0x0F)
            {
                // palette set
                return GetTexture(fileID);
            }

            else if (fileID >> 24 == 0x05)
            {
                // surface texture
                var surfaceTexture = DatManager.PortalDat.ReadFromDat<SurfaceTexture>(fileID);
                return GetTexture(surfaceTexture, customPaletteColors, useCache);
            }
            else if (fileID >> 24 == 0x06)
            {
                // texture
                return GetTexture(fileID, customPaletteColors, useCache);
            }

            else if (fileID >> 24 == 0x08)
            {
                // surface
                var surface = DatManager.PortalDat.ReadFromDat<Surface>(fileID);

                if (surface.ColorValue != 0)
                {
                    // swatch
                    var swatch = new Texture2D(GameView.Instance.GraphicsDevice, 1, 1);
                    var a = surface.ColorValue >> 24;
                    var r = (surface.ColorValue >> 16) & 0xFF;
                    var g = (surface.ColorValue >> 8) & 0xFF;
                    var b = surface.ColorValue & 0xFF;
                    a = 0;
                    swatch.SetData(new Microsoft.Xna.Framework.Color[] { new Microsoft.Xna.Framework.Color(r, g, b, a) });
                    return swatch;
                }

                uint textureId = surface.OrigTextureId;
                if (textureMapChanges != null)
                {
                    var tmChange = textureMapChanges.Where(t => t.OldTexture == surface.OrigTextureId).FirstOrDefault();
                    if (tmChange != null)
                        textureId = tmChange.NewTexture;
                }
                var surfaceTexture = DatManager.PortalDat.ReadFromDat<SurfaceTexture>(textureId);

                return GetTexture(surface, surfaceTexture.Textures[0], customPaletteColors, useCache);
            }
            return null;
        }

        /// <summary>
        /// Helper function to determine if we use the "clean" surface from the portal.dat, or the customized one with a texture swap.
        /// </summary>
        /// <param name="surfaceID"></param>
        /// <param name="surfaces"></param>
        /// <returns></returns>
        private static FileTypes.Surface GetCorrectSurface(uint surfaceID, List<FileTypes.Surface> surfaces = null)
        {
            var datSurface = DatManager.PortalDat.ReadFromDat<Surface>(surfaceID);
            FileTypes.Surface surface = new FileTypes.Surface(datSurface);

            if (surfaces != null && surfaces.Count > 0)
            {
                foreach (var s in surfaces)
                {
                    if (s._surface.Id == surfaceID)
                        surface = s;
                }
            }

            return surface;
        }

        private static Texture2D GetTexture(uint textureID, Dictionary<int, uint> customPaletteColors = null, bool useCache = true)
        {
            if (useCache && Textures.TryGetValue(textureID, out var cached))
                return cached;

            var texture = LoadTexture(textureID);
            if(useCache)
                Textures.Add(textureID, texture);
            return texture;
        }

        private static Texture2D GetTexture(SurfaceTexture surfaceTexture, Dictionary<int, uint> customPaletteColors = null, bool useCache = true)
        {
            return GetTexture(surfaceTexture.Textures[0], customPaletteColors, useCache);
        }

        private static Texture2D GetTexture(Surface surface, uint textureID, Dictionary<int, uint> customPaletteColors = null, bool useCache = true)
        {
            if (useCache && Textures.TryGetValue(textureID, out var cached))
                return cached;

            var texture = LoadTexture(surface, textureID, false, customPaletteColors);
            if(useCache)
                Textures.Add(textureID, texture);
            return texture;
        }

        public static Texture2D GenerateMipMaps(byte[] data, int size)
        {
            var source = new Texture2D(GraphicsDevice, size, size, false, SurfaceFormat.Color);
            source.SetData(data);

            var texture = new Texture2D(GraphicsDevice, size, size, true, SurfaceFormat.Color);
            texture.SetData(0, null, data, 0, data.Length);

            var miplevel = new List<byte[]>();
            var mipsize = size / 2;
            while (true)
            {
                var mipmap = GenerateMipMap(source, mipsize);
                var mipdata = new byte[mipsize * mipsize * 4];
                mipmap.GetData(mipdata);
                miplevel.Add(mipdata);
                if (mipsize > 1)
                    mipsize /= 2;
                else
                    break;
            };

            for (var i = 0; i < miplevel.Count; i++)
                texture.SetData(i + 1, null, miplevel[i], 0, miplevel[i].Length);

            return texture;
        }

        public static RenderTarget2D GenerateMipMap(Texture2D source, int size)
        {
            var renderTarget = new RenderTarget2D(GraphicsDevice, size, size);
            GraphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.Begin();
            spriteBatch.Draw(source, new Microsoft.Xna.Framework.Rectangle(0, 0, size, size), Microsoft.Xna.Framework.Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            return renderTarget;
        }
    }
}

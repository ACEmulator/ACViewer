using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

using ACViewer.Render;

namespace ACViewer
{
    /// <summary>
    /// Thanks to David Simpson for his early work on Dereth Cartopgrahy and his CellCracker tool which the logic for the map shading is lifted from
    /// </summary>
    public class Mapper
    {
        public DirectBitmap MapImage { get; set; }

        private class LandData
        {
            public ushort Type { get; set; }
            public int Z { get; set; }
            public bool Used { get; set; }
            public bool Blocked { get; set; }   // Can't walk on
        }

        // each landblock is 9x9 points, with the edge points being shared between neighbor landblocks.
        // 255 * 8 + 1, the extra 1 is for the last edge.
        const int LANDSIZE = 2041;

        // The following constants change how the lighting works.  It is easy to wash out
        // the bright whites of the snow, so be careful.

        // Incresing COLORCORRECTION makes the base color more prominant.
        const double COLORCORRECTION = 70.0;

        // Increasing LIGHTCORRECTION increases the contrast between steep and flat slopes.
        const double LIGHTCORRECTION = 2.25;

        // Increasing AMBIENTLIGHT makes everyting brighter.
        const double AMBIENTLIGHT = 64.0;

        private LandData[,] land { get; set; } = new LandData[LANDSIZE, LANDSIZE];

        const int LUMINANCE = 100;

        public int FoundLandblocks { get; set; }

        public Mapper(List<Color> MapColors = null)
        {
            FoundLandblocks = 0;

            for (var x = 0; x < LANDSIZE; x++)
            {
                for (var y = 0; y < LANDSIZE; y++)
                    land[x, y] = new LandData();
            }
            
            Parallel.For(0, 255 * 255, i =>
            {
                var block_x = i / 255;
                var block_y = i % 255;
                
                var key = (uint)(block_x << 24 | block_y << 16 | 0xFFFF);

                CellLandblock landblock = DatManager.CellDat.ReadFromDat<CellLandblock>(key);

                int startX = block_x * 8;
                int startY = LANDSIZE - block_y * 8 - 1;

                for (var x = 0; x < 9; x++)
                {
                    for (var y = 0; y < 9; y++)
                    {
                        var type = landblock.Terrain[x * 9 + y];
                        var newZ = landblock.Height[x * 9 + y];

                        // Write new data point
                        land[startY - y, startX + x].Type = type;
                        land[startY - y, startX + x].Z = GetLandheight(newZ);
                        land[startY - y, startX + x].Used = true;
                        uint itex = (uint)((type >> 2) & 0x3F);
                        if (itex < 16 || itex > 20)
                            land[startY - y, startX + x].Blocked = false;
                        else
                            land[startY - y, startX + x].Blocked = true;
                    }
                }

                FoundLandblocks++;
            });

            CreateMap();
        }

        private void CreateMap()
        {
            var emptyColor = Color.LimeGreen; // #32cd32

            double[] lightVector = new double[3] { -1.0, -1.0, 0.0 };
            byte[,,] topo = new byte[LANDSIZE, LANDSIZE, 3];

            List<Color> landColor = GetMapColors();

            Parallel.For(0, LANDSIZE * LANDSIZE, i =>
            {
                double[] v = new double[3];
                ushort type;
                double light;

                var x = i % LANDSIZE;
                var y = i / LANDSIZE;

                if (land[y, x].Used)
                {
                    // Calculate normal by using surrounding z values, if they exist
                    v[0] = 0.0;
                    v[1] = 0.0;
                    v[2] = 0.0;
                    if ((x < LANDSIZE - 1) && (y < LANDSIZE - 1))
                    {
                        if (land[y, x + 1].Used && land[y + 1, x].Used)
                        {
                            v[0] -= land[y, x + 1].Z - land[y, x].Z;
                            v[1] -= land[y + 1, x].Z - land[y, x].Z;
                            v[2] += 12.0;
                        }
                    }
                    if ((x > 0) && (y < LANDSIZE - 1))
                    {
                        if (land[y, x - 1].Used && land[y + 1, x].Used)
                        {
                            v[0] += land[y, x - 1].Z - land[y, x].Z;
                            v[1] -= land[y + 1, x].Z - land[y, x].Z;
                            v[2] += 12.0;
                        }
                    }
                    if ((x > 0) && (y > 0))
                    {
                        if (land[y, x - 1].Used && land[y - 1, x].Used)
                        {
                            v[0] += land[y, x - 1].Z - land[y, x].Z;
                            v[1] += land[y - 1, x].Z - land[y, x].Z;
                            v[2] += 12.0;
                        }
                    }
                    if ((x < LANDSIZE - 1) && (y > 0))
                    {
                        if (land[y, x + 1].Used && land[y - 1, x].Used)
                        {
                            v[0] -= land[y, x + 1].Z - land[y, x].Z;
                            v[1] += land[y - 1, x].Z - land[y, x].Z;
                            v[2] += 12.0;
                        }
                    }

                    // Check for road bit(s)
                    if ((land[y, x].Type & 0x0003) != 0)
                        type = 32;
                    else
                        type = (ushort)((land[y, x].Type & 0xFF) >> 2);

                    // Calculate lighting scalar
                    light = (((lightVector[0] * v[0] + lightVector[1] * v[1] + lightVector[2] * v[2]) /
                        Math.Sqrt((lightVector[0] * lightVector[0] + lightVector[1] * lightVector[1] + lightVector[2] * lightVector[2]) *
                        (v[0] * v[0] + v[1] * v[1] + v[2] * v[2]))) * 128.0 + 128.0) * LIGHTCORRECTION + AMBIENTLIGHT;

                    // Apply lighting scalar to base colors
                    double r = (landColor[type].R * COLORCORRECTION / 100) * light / 256.0;
                    double g = (landColor[type].G * COLORCORRECTION / 100) * light / 256.0;
                    double b = (landColor[type].B * COLORCORRECTION / 100) * light / 256.0;
                    r = ColorCheck(r);
                    g = ColorCheck(g);
                    b = ColorCheck(b);

                    topo[y, x, 0] = (byte)r;
                    topo[y, x, 1] = (byte)g;
                    topo[y, x, 2] = (byte)b;
                }
                else
                {
                    // If data is not present for a point on the map, the resultant pixel is green
                    topo[y, x, 0] = emptyColor.R;  // R
                    topo[y, x, 1] = emptyColor.G;  // G
                    topo[y, x, 2] = emptyColor.B;  // B
                }
            });
            
            MapImage = new DirectBitmap(LANDSIZE, LANDSIZE);

            for (var y = 0; y < LANDSIZE; y++)
            {
                for (var x = 0; x < LANDSIZE; x++)
                {
                    Color pixColor = Color.FromArgb(topo[y, x, 0], topo[y, x, 1], topo[y, x, 2]);
                    MapImage.SetPixel(x, y, pixColor);
                }
            }
        }

        /// <summary>
        /// Sanity check to make sure our colors are in-bounds.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private double ColorCheck(double color)
        {
            if (color > 255.0)
                return 255;
            else if (color < 0.0)
                return 0;
            return color;
        }

        private List<Color> GetMapColors()
        {
            uint RegionID = 0x13000000;
            var Region = DatManager.PortalDat.ReadFromDat<RegionDesc>(RegionID);
            Color[] landColors = new Color[Region.TerrainInfo.LandSurfaces.TexMerge.TerrainDesc.Count];
            for (var i = 0; i < Region.TerrainInfo.LandSurfaces.TexMerge.TerrainDesc.Count; i++)
            {
                var t = Region.TerrainInfo.LandSurfaces.TexMerge.TerrainDesc[i];
                var surfaceId = t.TerrainTex.TexGID;
                SurfaceTexture st;
                st = DatManager.PortalDat.ReadFromDat<SurfaceTexture>(surfaceId);
                var textureId = st.Textures[st.Textures.Count - 1];
                var texture = DatManager.PortalDat.ReadFromDat<Texture>(textureId);
                landColors[i] = GetAverageColor(texture);
            }
            return landColors.ToList();
        }

        private Color GetAverageColor(Texture image)
        {
            if (image == null)
                return Color.FromArgb(0, 255, 0); // TRANSPARENT

            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color clr = GetPixel(image, x, y);
                    
                    // Is the A8R8G8B8 loading colors properly?
                    r += clr.B; // BLUE??
                    g += clr.G;
                    b += clr.R; // RED??

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;
            return Color.FromArgb(r, g, b);
        }

        private Color GetPixel(Texture texture, int x, int y)
        {
            var offset = (y * texture.Width + x) * 4;

            var r = texture.SourceData[offset + 2];
            var g = texture.SourceData[offset + 1];
            var b = texture.SourceData[offset];

            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Functions like the Region.LandDefs.Land_Height_Table from (client_)portal.dat 0x13000000
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        private int GetLandheight(byte height)
        {
            if (height <= 200)
                return height * 2;
            else if (height <= 240)
                return 400 + (height - 200) * 4;
            else if (height <= 250)
                return 560 + (height - 240) * 8;
            else
                return 640 + (height - 250) * 10;
        }
    }
}

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

        // Increasing COLORCORRECTION makes the base color more prominant.
        const float COLORCORRECTION = 0.7f;

        // Increasing LIGHTCORRECTION increases the contrast between steep and flat slopes.
        const float LIGHTCORRECTION = 2.25f;

        // Increasing AMBIENTLIGHT makes everyting brighter.
        const float AMBIENTLIGHT = 0.25f;

        private LandData[,] land { get; set; } = new LandData[LANDSIZE, LANDSIZE];

        public int FoundLandblocks { get; set; }

        public Mapper()
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
                if (DatManager.CellDat.AllFiles.ContainsKey(key)) // Ensures we either have a full cell, or prevents crashes
                {
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
                }
            });

            CreateMap();
        }

        private void CreateMap()
        {
            var emptyColor = Color.LimeGreen; // #32cd32

            var lightVector = new float[3] { -1.0f, -1.0f, 0.0f };
            var topo = new byte[LANDSIZE, LANDSIZE, 3];

            List<Color> landColor = GetMapColors();

            Parallel.For(0, LANDSIZE * LANDSIZE, i =>
            {
                var x = i / LANDSIZE;
                var y = i % LANDSIZE;

                var v = new float[3];

                if (land[y, x].Used)
                {
                    // Calculate normal by using surrounding z values, if they exist
                    if ((x < LANDSIZE - 1) && (y < LANDSIZE - 1))
                    {
                        if (land[y, x + 1].Used && land[y + 1, x].Used)
                        {
                            v[0] -= land[y, x + 1].Z - land[y, x].Z;
                            v[1] -= land[y + 1, x].Z - land[y, x].Z;
                            v[2] += 12.0f;
                        }
                    }
                    if ((x > 0) && (y < LANDSIZE - 1))
                    {
                        if (land[y, x - 1].Used && land[y + 1, x].Used)
                        {
                            v[0] += land[y, x - 1].Z - land[y, x].Z;
                            v[1] -= land[y + 1, x].Z - land[y, x].Z;
                            v[2] += 12.0f;
                        }
                    }
                    if ((x > 0) && (y > 0))
                    {
                        if (land[y, x - 1].Used && land[y - 1, x].Used)
                        {
                            v[0] += land[y, x - 1].Z - land[y, x].Z;
                            v[1] += land[y - 1, x].Z - land[y, x].Z;
                            v[2] += 12.0f;
                        }
                    }
                    if ((x < LANDSIZE - 1) && (y > 0))
                    {
                        if (land[y, x + 1].Used && land[y - 1, x].Used)
                        {
                            v[0] -= land[y, x + 1].Z - land[y, x].Z;
                            v[1] += land[y - 1, x].Z - land[y, x].Z;
                            v[2] += 12.0f;
                        }
                    }

                    // Check for road bit(s)
                    var type = 0;
                    if ((land[y, x].Type & 0x0003) != 0)
                        type = 32;
                    else
                        type = (ushort)((land[y, x].Type & 0xFF) >> 2);

                    // Calculate lighting scalar
                    float light = (((lightVector[0] * v[0] + lightVector[1] * v[1] + lightVector[2] * v[2]) /
                        (float)Math.Sqrt((lightVector[0] * lightVector[0] + lightVector[1] * lightVector[1] + lightVector[2] * lightVector[2]) *
                        (v[0] * v[0] + v[1] * v[1] + v[2] * v[2])) * 0.3f + 0.5f) * LIGHTCORRECTION + AMBIENTLIGHT) * COLORCORRECTION;

                    // Apply lighting scalar to base colors
                    float r = landColor[type].R * light;
                    float g = landColor[type].G * light;
                    float b = landColor[type].B * light;

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
        private float ColorCheck(float color)
        {
            return Math.Clamp(color, 0.0f, 255.0f);
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

            // Used for tally
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

            // Calculate average
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

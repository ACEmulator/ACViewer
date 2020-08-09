using System;
using ACE.Entity.Enum;

namespace ACViewer.Render
{
    public class PixelFormatDesc
    {
        public SurfacePixelFormat Format;  // PixelFormatID
        public uint Flags;
        public uint FourCC;
        public byte BitsPerPixel;
        public uint RedBitMask;
        public uint GreenBitMask;
        public uint BlueBitMask;
        public uint AlphaBitMask;
        public byte RedBitCount;
        public byte GreenBitCount;
        public byte BlueBitCount;
        public byte AlphaBitCount;
        public byte RedBitOffset;
        public byte GreenBitOffset;
        public byte BlueBitOffset;
        public byte AlphaBitOffset;
        public uint RedMax;
        public uint GreenMax;
        public uint BlueMax;
        public uint AlphaMax;

        public PixelFormatDesc(SurfacePixelFormat format)
        {
            SetFormat(format);
        }

        public bool SetFormat(SurfacePixelFormat format)
        {
            if (format <= SurfacePixelFormat.PFID_P8)
            {
                if (format == SurfacePixelFormat.PFID_P8)
                {
                    Flags = 64;
                    BitsPerPixel = 8;
                    CalcBitOffsets();
                    return true;
                }
                switch (format)
                {
                    case SurfacePixelFormat.PFID_A8R8G8B8:
                        Flags = 3;
                        BitsPerPixel = 32;
                        AlphaBitMask = 0xFF000000;
                        RedBitMask = 0xFF0000;
                        GreenBitMask = 0xFF00;
                        BlueBitMask = 0xFF;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_X8R8G8B8:
                        Flags = 1;
                        BitsPerPixel = 32;
                        AlphaBitMask = 0xFF000000;
                        RedBitMask = 0xFF0000;
                        GreenBitMask = 0xFF00;
                        BlueBitMask = 0xFF;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_R5G6B5:
                        Flags = 1;
                        BitsPerPixel = 16;
                        RedBitMask = 0xF800;
                        GreenBitMask = 0x7E0;
                        BlueBitMask = 0x1F;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_X1R5G5B5:
                        Flags = 1;
                        BitsPerPixel = 16;
                        RedBitMask = 0x7C00;
                        GreenBitMask = 0x3E0;
                        BlueBitMask = 0x1F;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_A1R5G5B5:
                        Flags = 3;
                        BitsPerPixel = 16;
                        AlphaBitMask = 0x8000;
                        RedBitMask = 0x7C00;
                        GreenBitMask = 0x3E0;
                        BlueBitMask = 0x1F;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_A4R4G4B4:
                        Flags = 3;
                        BitsPerPixel = 16;
                        AlphaBitMask = 0xF000;
                        RedBitMask = 0xF00;
                        GreenBitMask = 0xF0;
                        BlueBitMask = 0xF;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_X4R4G4B4:
                        Flags = 1;
                        BitsPerPixel = 16;
                        RedBitMask = 0xF00;
                        GreenBitMask = 0xF0;
                        BlueBitMask = 0xF;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_A2B10G10R10:
                        Flags = 3;
                        BitsPerPixel = 32;
                        AlphaBitMask = 0xC0000000;
                        BlueBitMask = 0x3FF00000;
                        GreenBitMask = 0xFFC00;
                        RedBitMask = 0x3FF;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_A8B8G8R8:
                        Flags = 3;
                        BitsPerPixel = 32;
                        AlphaBitMask = 0xFF000000;
                        BlueBitMask = 0xFF0000;
                        GreenBitMask = 0xFF00;
                        RedBitMask = 0xFF;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_X8B8G8R8:
                        Flags = 1;
                        BitsPerPixel = 32;
                        BlueBitMask = 0xFF0000;
                        GreenBitMask = 0xFF00;
                        RedBitMask = 0xFF;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_A2R10G10B10:
                        Flags = 3;
                        BitsPerPixel = 32;
                        AlphaBitMask = 0xC0000000;
                        RedBitMask = 0x3FF00000;
                        GreenBitMask = 0xFFC00;
                        BlueBitMask = 0x3FF;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_R8G8B8:
                        Flags = 1;
                        BitsPerPixel = 24;
                        RedBitMask = 0xFF0000;
                        GreenBitMask = 0xFF00;
                        BlueBitMask = 0xFF;
                        CalcBitOffsets();
                        return true;

                    case SurfacePixelFormat.PFID_A8:
                        Flags = 2;
                        BitsPerPixel = 8;
                        AlphaBitMask = 0xFF;
                        CalcBitOffsets();
                        return true;

                    default:
                        return false;
                }
            }
            if (format > SurfacePixelFormat.PFID_CUSTOM_A8B8G8R8)
            {
                if (format <= SurfacePixelFormat.PFID_DXT1)
                {
                    if (format == SurfacePixelFormat.PFID_DXT1)
                    {
                        Flags = 4;
                        FourCC = (uint)SurfacePixelFormat.PFID_DXT1;
                        BitsPerPixel = 4;
                        CalcBitOffsets();
                        return true;
                    }
                    if (format > SurfacePixelFormat.PFID_CUSTOM_LSCAPE_ALPHA)
                    {
                        if (format == SurfacePixelFormat.PFID_CUSTOM_RAW_JPEG)
                        {
                            Flags = 17;
                            CalcBitOffsets();
                            return true;
                        }
                        return false;
                    }
                    if (format == SurfacePixelFormat.PFID_CUSTOM_LSCAPE_ALPHA)
                    {
                        Flags = 2;
                        AlphaBitMask = 0xFF;
                        BitsPerPixel = 8;
                        CalcBitOffsets();
                        return true;
                    }
                    if (format == SurfacePixelFormat.PFID_CUSTOM_B8G8R8)
                    {
                        Flags = 1;
                        BitsPerPixel = 24;
                        RedBitMask = 0xFF;
                        GreenBitMask = 0xFF00;
                        BlueBitMask = 0xFF0000;
                        CalcBitOffsets();
                        return true;
                    }
                    if (format == SurfacePixelFormat.PFID_CUSTOM_LSCAPE_R8G8B8)
                    {
                        Flags = 1;
                        BitsPerPixel = 24;
                        RedBitMask = 0xFF0000;
                        GreenBitMask = 0xFF00;
                        BlueBitMask = 0xFF;
                        CalcBitOffsets();
                        return true;
                    }
                    return false;
                }
                if (format > SurfacePixelFormat.PFID_DXT4)
                {
                    if (format != SurfacePixelFormat.PFID_DXT5)
                        return false;

                    FourCC = (uint)SurfacePixelFormat.PFID_DXT5;
                }
                else if (format == SurfacePixelFormat.PFID_DXT4)
                {
                    FourCC = (uint)SurfacePixelFormat.PFID_DXT4;
                }
                else if (format == SurfacePixelFormat.PFID_DXT2)
                {
                    FourCC = (uint)SurfacePixelFormat.PFID_DXT2;
                }
                else
                {
                    if (format != SurfacePixelFormat.PFID_DXT3)
                        return false;

                    FourCC = (uint)SurfacePixelFormat.PFID_DXT3;
                }
                Flags = 4;
                BitsPerPixel = 8;
                CalcBitOffsets();
                return true;
            }
            if (format == SurfacePixelFormat.PFID_CUSTOM_A8B8G8R8)
            {
                Flags = 3;
                BitsPerPixel = 32;
                RedBitMask = 0xFF;
                GreenBitMask = 0xFF00;
                BlueBitMask = 0xFF0000;
                AlphaBitMask = 0xFF000000;
                CalcBitOffsets();
                return true;
            }
            else
            {
                switch (format)
                {
                    case SurfacePixelFormat.PFID_INDEX16:
                        Flags = 64;
                        BitsPerPixel = 16;
                        CalcBitOffsets();
                        return true;
                    case SurfacePixelFormat.PFID_V8U8:
                        Flags = 9;
                        BitsPerPixel = 16;
                        RedBitMask = 0xFF00;
                        GreenBitMask = 0xFF;
                        BlueBitMask = 0;
                        CalcBitOffsets();
                        return true;
                    case SurfacePixelFormat.PFID_D32:
                        Flags = 2;
                        BitsPerPixel = 32;
                        AlphaBitMask = UInt32.MaxValue;
                        CalcBitOffsets();
                        return true;
                    case SurfacePixelFormat.PFID_D15S1:
                        Flags = 2;
                        BitsPerPixel = 16;
                        AlphaBitMask = 0xFFFE;
                        CalcBitOffsets();
                        return true;
                    case SurfacePixelFormat.PFID_D16_LOCKABLE:
                    case SurfacePixelFormat.PFID_D16:
                        Flags = 2;
                        BitsPerPixel = 16;
                        AlphaBitMask = 0xFFFF;
                        CalcBitOffsets();
                        return true;
                    case SurfacePixelFormat.PFID_D24S8:
                    case SurfacePixelFormat.PFID_D24X8:
                    case SurfacePixelFormat.PFID_D24X4S4:
                        Flags = 2;
                        BitsPerPixel = 32;
                        AlphaBitMask = 0xFFFFFF00;
                        CalcBitOffsets();
                        return true;
                    case SurfacePixelFormat.PFID_CUSTOM_R8G8B8A8:
                        Flags = 3;
                        BitsPerPixel = 32;
                        RedBitMask = 0xFF000000;
                        GreenBitMask = 0xFF0000;
                        BlueBitMask = 0XFF00;
                        AlphaBitMask = 0xFF;
                        CalcBitOffsets();
                        return true;
                    default:
                        return false;
                }
            }
        }

        public void CalcBitOffsets()
        {
            RedBitOffset = CalcBitOffset(RedBitMask);
            GreenBitOffset = CalcBitOffset(GreenBitMask);
            BlueBitOffset = CalcBitOffset(BlueBitMask);
            AlphaBitOffset = CalcBitOffset(AlphaBitMask);
        }

        public byte CalcBitOffset(uint mask)
        {
            byte offset = 0;
            while (((mask >> offset) & 1) == 0)
            {
                offset++;
                if (offset >= 0x20)
                {
                    offset = 0;
                    break;
                }
            }
            return offset;
        }
    }
}

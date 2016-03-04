using System;
using System.Collections.Generic;
using System.Text;
using PaintDotNet;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace pyrochild.effects.common
{
    public static class Extensions
    {
        public static Color ToOpaqueColor(this Color color)
        {
            return Color.FromArgb(255, color);
        }

        unsafe public static void Checker(this Surface surface)
        {
            int xOffset = 0, yOffset = 0;
            for (int y = 0; y < surface.Height; y++)
            {
                ColorBgra* dstPtr = surface.GetRowAddressUnchecked(y);
                for (int x = 0; x < surface.Width; x++)
                {
                    byte v = (byte)(((((x + xOffset) ^ (y + yOffset)) & 8) * 8) + 0xbf);
                    *dstPtr = ColorBgra.FromBgra(v, v, v, 0xff);
                    dstPtr++;
                }
            }
        }

        public static byte ClampToByte(this int val)
        {
            if (val > 255) return 255;
            if (val < 0) return 0;
            return (byte)val;
        }

        public static byte ClampToByte(this double val)
        {
            if (val > 255) return 255;
            if (val < 0) return 0;
            return (byte)val;
        }

        public static byte ClampToByte(this float val)
        {
            if (val > 255) return 255;
            if (val < 0) return 0;
            return (byte)val;
        }

        public static string StripIllegalPathChars(this string str)
        {
            StringBuilder sb = new StringBuilder(str);
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                sb.Replace(c.ToString(), "");
            }
            return sb.ToString();
        }

        public static PdnRegion GetOutline(this PdnRegion region, RectangleF bounds, float scalefactor)
        {
            GraphicsPath path = new GraphicsPath();

            PdnRegion region2 = region.Clone();

            Matrix scalematrix = new Matrix(
                bounds,
                new PointF[]{
                    new PointF(bounds.Left, bounds.Top),
                    new PointF(bounds.Right*scalefactor, bounds.Top),
                    new PointF(bounds.Left, bounds.Bottom*scalefactor)
                });
            region2.Transform(scalematrix);

            foreach (RectangleF rect in region2.GetRegionScans())
            {
                path.AddRectangle(RectangleF.Inflate(rect, 1, 1));
            }

            PdnRegion retval = new PdnRegion(path);
            retval.Exclude(region2);

            return retval;
        }
    }
}
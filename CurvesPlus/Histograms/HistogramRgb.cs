using System;
using System.Drawing;
using PaintDotNet;
using PaintDotNet.Imaging;

namespace pyrochild.effects.common
{
    public partial class Histograms
    {
        public sealed class HistogramRgb
            : Histogram
        {
            public HistogramRgb()
                : base(3, 256)
            {
            }

            protected override unsafe void AddRegionRectangleToHistogram(RegionPtr<ColorBgra32> region, Rectangle rect)
            {
                long[] histogramR = histogram[0];
                long[] histogramG = histogram[1];
                long[] histogramB = histogram[2];
                RegionPtr<ColorBgra> regionBgra = region.Cast<ColorBgra>();

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* ptr = regionBgra.Rows[y].Ptr + rect.Left;
                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        ++histogramB[ptr->B];
                        ++histogramG[ptr->G];
                        ++histogramR[ptr->R];
                        ++ptr;
                    }
                }
            }
        }
    }
}
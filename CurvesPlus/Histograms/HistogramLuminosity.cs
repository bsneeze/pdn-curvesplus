using System;
using System.Drawing;
using PaintDotNet;
using PaintDotNet.Imaging;

namespace pyrochild.effects.common
{
    public partial class Histograms
    {
        public sealed class HistogramLuminosity
            : Histogram
        {
            public HistogramLuminosity()
                : base(1, 256)
            {
            }

            protected override unsafe void AddRegionRectangleToHistogram(RegionPtr<ColorBgra32> region, Rectangle rect)
            {
                long[] histogramLuminosity = histogram[0];
                RegionPtr<ColorBgra> regionBgra = region.Cast<ColorBgra>();

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {

                    ColorBgra* ptr = regionBgra.Rows[y].Ptr + rect.Left;
                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        ++histogramLuminosity[ptr->GetIntensityByte()];
                        ++ptr;
                    }
                }
            }
        }
    }
}
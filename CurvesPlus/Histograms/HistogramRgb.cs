using System;
using System.Drawing;
using PaintDotNet;

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

            protected override unsafe void AddSurfaceRectangleToHistogram(Surface surface, Rectangle rect)
            {
                long[] histogramR = histogram[0];
                long[] histogramG = histogram[1];
                long[] histogramB = histogram[2];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* ptr = surface.GetPointAddressUnchecked(rect.Left, y);
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
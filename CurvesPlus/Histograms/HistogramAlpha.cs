using System;
using System.Drawing;
using PaintDotNet;

namespace pyrochild.effects.common
{
    public partial class Histograms
    {
        public sealed class HistogramAlpha
            : Histogram
        {
            public HistogramAlpha()
                : base(1, 256)
            {
            }

            protected override unsafe void AddSurfaceRectangleToHistogram(Surface surface, Rectangle rect)
            {
                long[] histogramAlpha = histogram[0];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* ptr = surface.GetPointAddressUnchecked(rect.Left, y);
                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        ++histogramAlpha[ptr->A];
                        ++ptr;
                    }
                }
            }
        }
    }
}
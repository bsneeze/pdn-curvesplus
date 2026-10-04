using System;
using System.Drawing;
using PaintDotNet;
using PaintDotNet.Imaging;

namespace pyrochild.effects.common
{
    public partial class Histograms
    {
        public sealed class HistogramCmyk
            : Histogram
        {
            public HistogramCmyk()
                : base(4, 256)
            {
            }

            protected override unsafe void AddRegionRectangleToHistogram(RegionPtr<ColorBgra32> region, Rectangle rect)
            {
                long[] histogramC = histogram[0];
                long[] histogramM = histogram[1];
                long[] histogramY = histogram[2];
                long[] histogramK = histogram[3];
                RegionPtr<ColorBgra> regionBgra = region.Cast<ColorBgra>();

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* ptr = regionBgra.Rows[y].Ptr + rect.Left;
                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        byte C = (byte)(255 - ptr->R);
                        byte M = (byte)(255 - ptr->G);
                        byte Y = (byte)(255 - ptr->B);
                        byte K = (byte)Math.Min(Math.Min(C, M), Y);
                        C -= K;
                        M -= K;
                        Y -= K;
                        ++histogramC[C];
                        ++histogramM[M];
                        ++histogramY[Y];
                        ++histogramK[K];
                        ++ptr;
                    }
                }
            }
        }
    }
}
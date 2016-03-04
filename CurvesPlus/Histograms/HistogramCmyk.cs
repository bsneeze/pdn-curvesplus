using System;
using System.Drawing;
using PaintDotNet;

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

            protected override unsafe void AddSurfaceRectangleToHistogram(Surface surface, Rectangle rect)
            {
                long[] histogramC = histogram[0];
                long[] histogramM = histogram[1];
                long[] histogramY = histogram[2];
                long[] histogramK = histogram[3];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* ptr = surface.GetPointAddressUnchecked(rect.Left, y);
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
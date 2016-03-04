using System;
using System.Drawing;
using PaintDotNet;
using pyrochild.effects.common;

namespace pyrochild.effects.common
{
    public partial class Histograms
    {
        public sealed class HistogramHsv
            : Histogram
        {
            public HistogramHsv(/*bool SkipZeroSat*/)
                : base(3, 361)
            {
                //this.skipzerosat = SkipZeroSat;
            }

            //private bool skipzerosat;

            protected override unsafe void AddSurfaceRectangleToHistogram(Surface surface, Rectangle rect)
            {
                long[] histogramH = histogram[0];
                long[] histogramS = histogram[1];
                long[] histogramV = histogram[2];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* ptr = surface.GetPointAddressUnchecked(rect.Left, y);
                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        HsvColor hsvColor = HsvColor.FromColor(ptr->ToColor());

                        //if (!skipzerosat && hsvColor.Saturation > 0)
                        {
                            ++histogramH[hsvColor.Hue];
                        }
                        ++histogramS[hsvColor.Saturation];
                        ++histogramV[hsvColor.Value];
                        ++ptr;
                    }
                }
            }
        }
    }
}
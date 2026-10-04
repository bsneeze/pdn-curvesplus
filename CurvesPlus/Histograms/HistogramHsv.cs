using System;
using System.Drawing;
using PaintDotNet;
using PaintDotNet.Imaging;
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

            protected override unsafe void AddRegionRectangleToHistogram(RegionPtr<ColorBgra32> region, Rectangle rect)
            {
                long[] histogramH = histogram[0];
                long[] histogramS = histogram[1];
                long[] histogramV = histogram[2];
                RegionPtr<ColorBgra> regionBgra = region.Cast<ColorBgra>();

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* ptr = regionBgra.Rows[y].Ptr + rect.Left;
                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        ColorHsv96Float hsvColor = (*ptr).ToHsvColor();

                        //if (!skipzerosat && hsvColor.Saturation > 0)
                        {
                            ++histogramH[(int)Math.Round(hsvColor.Hue)];
                        }
                        ++histogramS[(int)Math.Round(hsvColor.Saturation)];
                        ++histogramV[(int)Math.Round(hsvColor.Value)];
                        ++ptr;
                    }
                }
            }
        }
    }
}
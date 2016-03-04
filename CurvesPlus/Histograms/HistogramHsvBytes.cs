using System;
using System.Drawing;
using PaintDotNet;
using pyrochild.effects.common;

namespace pyrochild.effects.common
{
    public partial class Histograms
    {
        public sealed class HistogramHsvBytes
            : Histogram
        {
            public HistogramHsvBytes()
                : base(3, 256)
            {
            }

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
                        int h = 0;
                        int s = 0;
                        int v = 0;

                        int max = (ptr->R > ptr->G) ? ptr->R : ptr->G;
                        if (max < ptr->B) max = ptr->B;

                        if (max == 0)
                        {
                            // R, G, and B must be 0
                            // In this case, S is 0, and H is undefined.
                            // Using H = 0 is as good as any...
                            // And of course v = 0.

                            //since these are the initial values we do
                            //not need to do anything
                        }
                        else
                        {
                            v = max;

                            int min = (ptr->R < ptr->G) ? ptr->R : ptr->G;
                            if (min > ptr->B) min = ptr->B;

                            int delta = max - min;

                            if (delta == 0)
                            {
                                // R, G, and B must all the same.
                                // In this case, S is 0, and H is undefined.
                                // Using H = 0 is as good as any...
                                s = 0;
                                h = 0;
                            }
                            else
                            {
                                s = CommonUtil.IntDiv(255 * delta, max);

                                if (ptr->R == max)
                                {
                                    // Between Yellow and Magenta
                                    h = CommonUtil.IntDiv(255 * (ptr->G - ptr->B), delta);
                                }
                                else if (ptr->G == max)
                                {
                                    // Between Cyan and Yellow
                                    h = 512 + CommonUtil.IntDiv(255 * (ptr->B - ptr->R), delta);
                                }
                                else
                                {
                                    // Between Magenta and Cyan
                                    h = 1024 + CommonUtil.IntDiv(255 * (ptr->R - ptr->G), delta);
                                }

                                if (h < 0)
                                {
                                    h += 1536;
                                }

                                h = CommonUtil.IntDiv(h, 6);
                            }
                        }

                        ++histogramH[h];
                        ++histogramS[s];
                        ++histogramV[v];
                        ++ptr;
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using PaintDotNet;
using PaintDotNet.Imaging;

namespace pyrochild.effects.common
{
    public partial class Histograms
    {
        public abstract class Histogram
        {
            protected long[][] histogram;
            public long[][] HistogramValues
            {
                get
                {
                    return this.histogram;
                }

                set
                {
                    if (value != null)
                    {
                        if (value.Length == this.histogram.Length && value[0].Length == this.histogram[0].Length)
                        {
                            this.histogram = value;
                            OnHistogramUpdated();
                        }
                        else
                        {
                            throw new ArgumentException("value muse be an array of arrays of matching size", "value");
                        }
                    }
                }
            }

            public int Channels
            {
                get
                {
                    return this.histogram.Length;
                }
            }

            public int Entries
            {
                get
                {
                    return this.histogram[0].Length;
                }
            }

            protected internal Histogram(int channels, long entries)
            {
                this.histogram = new long[channels][];

                for (int channel = 0; channel < channels; ++channel)
                {
                    this.histogram[channel] = new long[entries];
                }
            }

            public event EventHandler HistogramChanged;
            protected void OnHistogramUpdated()
            {
                if (HistogramChanged != null)
                {
                    HistogramChanged(this, EventArgs.Empty);
                }
            }

            public long GetMax()
            {
                long max = -1;

                foreach (long[] channelHistogram in histogram)
                {
                    foreach (long i in channelHistogram)
                    {
                        if (i > max)
                        {
                            max = i;
                        }
                    }
                }

                return max;
            }

            protected void Clear()
            {
                histogram.Initialize();
            }

            // region must be the whole document: rects/scans index region.Rows in document coordinates.
            protected abstract void AddRegionRectangleToHistogram(RegionPtr<ColorBgra32> region, Rectangle rect);

            public void UpdateHistogram(RegionPtr<ColorBgra32> region)
            {
                Clear();
                AddRegionRectangleToHistogram(region, new Rectangle(0, 0, region.Width, region.Height));
                OnHistogramUpdated();
            }

            public void UpdateHistogram(RegionPtr<ColorBgra32> region, Rectangle rect)
            {
                Clear();
                AddRegionRectangleToHistogram(region, rect);
                OnHistogramUpdated();
            }

            public void UpdateHistogram(RegionPtr<ColorBgra32> region, PdnRegion roi)
            {
                Clear();

                foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
                {
                    AddRegionRectangleToHistogram(region, rect);
                }

                OnHistogramUpdated();
            }

            public void UpdateHistogram(RegionPtr<ColorBgra32> region, IEnumerable<Rectangle> scans)
            {
                Clear();

                foreach (Rectangle rect in scans)
                {
                    AddRegionRectangleToHistogram(region, rect);
                }

                OnHistogramUpdated();
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PaintDotNet;
using pyrochild.effects.common;

namespace pyrochild.effects.curvesplus
{
    public sealed class CurveControlHsv
        : CurveControl
    {
        public CurveControlHsv()
            : base(3, 256)
        {
            this.mask = new bool[3] { true, true, true };
            visualColors = new ColorBgra[] {     
                                               ColorBgra.Red,
                                               ColorBgra.Blue,
                                               ColorBgra.Black
                                           };
            channelNames = new string[]{
                "Hue",
                "Saturation",
                "Value"
            };
            ResetControlPoints();

            Histogram = new Histograms.HistogramHsvBytes();
        }

        public override ChannelMode ColorTransferMode
        {
            get
            {
                return ChannelMode.Hsv;
            }
        }

        public override Brush GetBrush(Color color, int channel, Rectangle rect, LinearGradientMode lgm)
        {
            if (channel == 0)
            {
                LinearGradientBrush brush = new LinearGradientBrush(rect, Color.Red, Color.Red, lgm);
                ColorBlend colorBlend = new ColorBlend();
                colorBlend.Positions = new float[]{
                    0.0f,
                    0.16666666666666666666666666666667f,
                    0.33333333333333333333333333333333f,
                    0.5f,
                    0.66666666666666666666666666666667f,
                    0.83333333333333333333333333333333f,
                    1.0f
                };
                switch (lgm)
                {
                    case LinearGradientMode.Horizontal:
                        colorBlend.Colors = new Color[] {
                            Color.FromArgb(color.A, 255, 0, 0),
                            Color.FromArgb(color.A, 255, 255, 0),
                            Color.FromArgb(color.A, 0, 255, 0),
                            Color.FromArgb(color.A, 0, 255, 255),
                            Color.FromArgb(color.A, 0, 0, 255),
                            Color.FromArgb(color.A, 255, 0, 255),
                            Color.FromArgb(color.A, 255, 0, 0)
                        };
                        break;
                    case LinearGradientMode.Vertical:
                        colorBlend.Colors = new Color[] {
                            Color.FromArgb(255, 0, 0),
                            Color.FromArgb(255, 0, 255),
                            Color.FromArgb(0, 0, 255),
                            Color.FromArgb(0, 255, 255),
                            Color.FromArgb(0, 255, 0),
                            Color.FromArgb(255, 255, 0),
                            Color.FromArgb(255, 0, 0)
                        };
                        break;
                }
                brush.InterpolationColors = colorBlend;
                return brush;
            }
            return new SolidBrush(color);
        }
    }
}

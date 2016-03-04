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
    /// <summary>
    /// Curve control specialization for Alpha curves
    /// </summary>
    public sealed class CurveControlAlpha
        : CurveControl
    {
        public CurveControlAlpha()
            : base(1, 256)
        {
            this.mask = new bool[1] { true };
            visualColors = new ColorBgra[] {     
                                               ColorBgra.Black
            };
            channelNames = new string[]{
                "Alpha"
            };
            ResetControlPoints();

            Histogram = new Histograms.HistogramAlpha();
        }

        public override ChannelMode ColorTransferMode
        {
            get
            {
                return ChannelMode.A;
            }
        }
    }
}

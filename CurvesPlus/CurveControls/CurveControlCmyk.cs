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
    /// Curve control specialization for CMYK curves
    /// </summary>
    public sealed class CurveControlCmyk
        : CurveControl
    {
        public CurveControlCmyk()
            : base(4, 256)
        {
            this.mask = new bool[4] { true, true, true, true };
            visualColors = new ColorBgra[] {     
                                               ColorBgra.Cyan,
                                               ColorBgra.Magenta,
                                               ColorBgra.Gold,
                                               ColorBgra.Black
                                           };
            channelNames = new string[] { "Cyan", "Magenta", "Yellow", "Key" };
            ResetControlPoints();

            Histogram = new Histograms.HistogramCmyk();
        }

        public override ChannelMode ColorTransferMode
        {
            get
            {
                return ChannelMode.Cmyk;
            }
        }
    }
}

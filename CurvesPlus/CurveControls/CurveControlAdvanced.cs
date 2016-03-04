using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using pyrochild.effects.common;
using PaintDotNet;

namespace pyrochild.effects.curvesplus
{
    public sealed class CurveControlAdvanced
        : CurveControl
    {
        public CurveControlAdvanced()
            : base(1, 256)
        {
            this.mask = new bool[1] { true };
            visualColors = new ColorBgra[] {     
                                               ColorBgra.Black
            };
            channelNames = new string[]{
                "Advanced"
            };
            ResetControlPoints();
        }

        public override ChannelMode ColorTransferMode
        {
            get
            {
                return ChannelMode.Advanced;
            }
        }
    }
}

/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
// Modifications Copyright © 2007-2016 Zach Walker                             //
/////////////////////////////////////////////////////////////////////////////////

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
    /// Curve control specialization for RGB curves
    /// </summary>
    public sealed class CurveControlRgb
        : CurveControl
    {
        public CurveControlRgb()
            : base(3, 256)
        {
            this.mask = new bool[3] { true, true, true };
            visualColors = new ColorBgra[] {
                ColorBgra.Red,
                ColorBgra.Green,
                ColorBgra.Blue
            };
            channelNames = new string[]{
                "Red",
                "Green",
                "Blue"
            };
            ResetControlPoints();

            Histogram = new Histograms.HistogramRgb();
        }

        public override ChannelMode ColorTransferMode
        {
            get
            {
                return ChannelMode.Rgb;
            }
        }
    }
}
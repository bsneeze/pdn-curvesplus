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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PaintDotNet;
using pyrochild.effects.common;

namespace pyrochild.effects.curvesplus
{
    /// <summary>
    /// Curve control specialized for luminosity
    /// </summary>
    public sealed class CurveControlLuminosity
        : CurveControl
    {
        public CurveControlLuminosity()
            : base(1, 256)
        {
            this.mask = new bool[1]{true};
            visualColors = new ColorBgra[]{     
                                              ColorBgra.Black
                                          };
            channelNames = new string[]{
                        "Luminosity"
            };
            ResetControlPoints();

            Histogram = new Histograms.HistogramLuminosity();
        }

        public override ChannelMode ColorTransferMode
        {
            get
            {
                return ChannelMode.L;
            }
        }
    }
}

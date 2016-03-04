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
using PaintDotNet;
using PaintDotNet.Effects;
using pyrochild.effects.common;

namespace pyrochild.effects.curvesplus
{
    [Serializable]
    public class ConfigToken
        : EffectConfigToken
    {
        public string Preset { get; set; }

        private SortedList<int, int>[] controlPoints;
        public SortedList<int, int>[] ControlPoints
        {
            get
            {
                return controlPoints;
            }

            set
            {
                uop = null;
                controlPoints = value;
            }
        }

        private ChannelMode colorTransferMode;
        public ChannelMode ColorTransferMode
        {
            get
            {
                return colorTransferMode;
            }

            set
            {
                uop = null;
                colorTransferMode = value;
            }
        }

        protected UnaryPixelHistogramOp uop;
        public UnaryPixelHistogramOp Uop
        {
            get
            {
                if (uop == null)
                {
                    uop = MakeUop();
                }

                return uop;
            }
        }

        public Channel InputMode;
        public Channel OutputMode;
        public CurveDrawMode CurveDrawMode;

        protected UnaryPixelHistogramOp MakeUop()
        {
            UnaryPixelHistogramOp uopRet;
            byte[][] transferCurves;
            int entries;

            switch (colorTransferMode)
            {
                case ChannelMode.Rgb:
                    UnaryPixelHistogramOps.RgbCurve rc = new UnaryPixelHistogramOps.RgbCurve();
                    transferCurves = new byte[][] { rc.CurveR, rc.CurveG, rc.CurveB };
                    entries = 256;
                    uopRet = rc;
                    break;

                case ChannelMode.L:
                    UnaryPixelHistogramOps.LuminosityCurve lc = new UnaryPixelHistogramOps.LuminosityCurve();
                    transferCurves = new byte[][] { lc.Curve };
                    entries = 256;
                    uopRet = lc;
                    break;

                case ChannelMode.A:
                    UnaryPixelHistogramOps.AlphaCurve ac = new UnaryPixelHistogramOps.AlphaCurve();
                    transferCurves = new byte[][] { ac.Curve };
                    entries = 256;
                    uopRet = ac;
                    break;

                case ChannelMode.Cmyk:
                    UnaryPixelHistogramOps.CmykCurve cc = new UnaryPixelHistogramOps.CmykCurve();
                    transferCurves = new byte[][] { cc.CurveC, cc.CurveM, cc.CurveY, cc.CurveK };
                    entries = 256;
                    uopRet = cc;
                    break;

                case ChannelMode.Advanced:
                    UnaryPixelHistogramOps.AdvancedCurve adc = new UnaryPixelHistogramOps.AdvancedCurve(InputMode, OutputMode);
                    transferCurves = new byte[][] { adc.Curve };
                    entries = 256;
                    uopRet = adc;
                    break;

                case ChannelMode.Hsv:
                    UnaryPixelHistogramOps.HsvCurve hc = new UnaryPixelHistogramOps.HsvCurve();
                    transferCurves = new byte[][] { hc.CurveH, hc.CurveS, hc.CurveV };
                    entries = 256;
                    uopRet = hc;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }


            int channels = transferCurves.Length;

            for (int channel = 0; channel < channels; ++channel)
            {
                SortedList<int, int> channelControlPoints;
                //lock (this)
                {
                    channelControlPoints = controlPoints[channel];
                }
                IList<int> xa = channelControlPoints.Keys;
                IList<int> ya = channelControlPoints.Values;
                int length = channelControlPoints.Count;
                switch (CurveDrawMode)
                {
                    case CurveDrawMode.Spline:
                        SplineInterpolator interpolator = new SplineInterpolator();

                        for (int i = 0; i < length; ++i)
                        {
                            interpolator.Add(xa[i], ya[i]);
                        }

                        for (int i = 0; i < entries; ++i)
                        {
                            transferCurves[channel][i] = interpolator.Interpolate(i).ClampToByte();
                        }
                        break;
                    case CurveDrawMode.Linear:
                        for (int i = 0; i < length - 1; i++)
                        {
                            for (int e = xa[i]; e <= xa[i + 1]; e++)
                            {
                                transferCurves[channel][e] = (byte)DoubleUtil.Lerp(ya[i], ya[i + 1], (double)(e - xa[i]) / (double)(xa[i + 1] - xa[i]));
                            }
                        }
                        break;
                    case CurveDrawMode.Pencil:
                        for (int i = 0; i < length; i++)
                        {
                            transferCurves[channel][i] = (byte)channelControlPoints[i];
                        }
                        break;
                }
            }

            return uopRet;
        }


        public override object Clone()
        {
            return new ConfigToken(this);
        }

        public ConfigToken()
        {
            controlPoints = new SortedList<int, int>[1];

            for (int i = 0; i < this.controlPoints.Length; ++i)
            {
                SortedList<int, int> newList = new SortedList<int, int>();

                newList.Add(0, 0);
                newList.Add(255, 255);
                controlPoints[i] = newList;
            }
            colorTransferMode = ChannelMode.L;
            InputMode = Channel.A;
            OutputMode = Channel.A;
            CurveDrawMode = CurveDrawMode.Spline;
            Preset = "Default";
        }

        protected ConfigToken(ConfigToken copyMe)
            : base(copyMe)
        {
            lock (copyMe)
            {
                this.uop = copyMe.Uop;
                this.colorTransferMode = copyMe.ColorTransferMode;

                this.controlPoints = new SortedList<int, int>[copyMe.controlPoints.Length];
                for (int i = 0; i < controlPoints.Length; i++)
                {
                    this.controlPoints[i] = new SortedList<int, int>(copyMe.controlPoints[i]);
                }

                this.InputMode = copyMe.InputMode;
                this.OutputMode = copyMe.OutputMode;
                this.CurveDrawMode = copyMe.CurveDrawMode;
                this.Preset = copyMe.Preset;
            }
        }
    }
}
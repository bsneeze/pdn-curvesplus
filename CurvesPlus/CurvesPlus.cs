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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PaintDotNet.Effects;
using PaintDotNet;
using pyrochild.effects.common;

namespace pyrochild.effects.curvesplus
{
    [PluginSupportInfo(typeof(PluginSupportInfo))]
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class CurvesPlus
        : Effect
    {
        public static string StaticDialogName
        {
            get
            {
                return StaticName + " by pyrochild";
            }
        }

        public static string StaticName
        {
            get
            {
                string s = "Curves+";
#if DEBUG
                s += " BETA";
#endif
                return s;
            }
        }

        public static Bitmap StaticIcon
        {
            get
            {
                return new Bitmap(typeof(CurvesPlus), "images.icon.png");
            }
        }

        public static string StaticSubMenuName
        {
            get
            {
                return null; 
            }
        }

        public CurvesPlus()
            : base(StaticName, StaticIcon, StaticSubMenuName, EffectFlags.Configurable)
        {
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            ConfigToken token = parameters as ConfigToken;

            if (token != null)
            {
                UnaryPixelHistogramOp uop = token.Uop;

                for (int i = startIndex; i < startIndex + length; ++i)
                {
                    uop.Apply(dstArgs.Surface, srcArgs.Surface, rois[i]);
                }
            }
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            return new ConfigDialog();
        }
    }
}
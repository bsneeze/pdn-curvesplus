/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
// Modifications Copyright � 2007-2016 Zach Walker                             //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.Imaging;
using PaintDotNet.Rendering;
using System.Drawing;
using pyrochild.effects.common;

namespace pyrochild.effects.curvesplus
{
    [PluginSupportInfo(typeof(PluginSupportInfo))]
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class CurvesPlus : BitmapEffect<ConfigToken>
    {
        UnaryPixelHistogramOp uop;

        public CurvesPlus() : base(StaticName, StaticIcon, StaticSubMenuName, BitmapEffectOptions.Create() with { IsConfigurable = true }) { }

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

        protected override IEffectConfigForm OnCreateConfigForm()
        {
            return new ConfigDialog();
        }

        protected override void OnSetToken(ConfigToken newToken)
        {
            base.OnSetToken(newToken);

            if (newToken != null)
            {
                uop = newToken.Uop;
            }
        }

        protected override unsafe void OnRender(IBitmapEffectOutput output)
        {
            if (uop == null)
            {
                return;
            }

            RectInt32 bounds = output.Bounds;

            using (IEffectInputBitmap<ColorBgra32> srcBitmap = Environment.GetSourceBitmapBgra32())
            using (IBitmapLock<ColorBgra32> srcLock = srcBitmap.Lock(new RectInt32(0, 0, srcBitmap.Size)))
            using (IBitmapLock<ColorBgra32> dstLock = output.LockBgra32())
            {
                RegionPtr<ColorBgra32> srcRegion = new RegionPtr<ColorBgra32>(srcLock.Buffer, srcLock.Size, srcLock.BufferStride);
                RegionPtr<ColorBgra32> dstRegion = new RegionPtr<ColorBgra32>(dstLock.Buffer, dstLock.Size, dstLock.BufferStride);

                uop.Apply(dstRegion, srcRegion, bounds);
            }
        }
    }
}

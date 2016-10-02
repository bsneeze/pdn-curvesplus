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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PaintDotNet;
using pyrochild.effects.common;

namespace pyrochild.effects.curvesplus
{    
    /// <summary>
    /// This class is for manipulation of transfer functions.
    /// It is intended for curve adjustment
    /// </summary>
    public abstract class CurveControl
        : UserControl
    {
        private System.ComponentModel.Container components = null;
        private int[] curvesInvalidRange = new int[] { int.MaxValue, int.MinValue };
        private Point lastMouseXY = new Point(int.MinValue, int.MinValue);
        private int lastKey = -1;
        private int lastValue = -1;
        private bool tracking = false;
        private Point[] ptSave;
        private int[] pointsNearMousePerChannel;
        private bool[] effectChannel;
        public CurveDrawMode curveDrawMode;
        private SortedList<int, int>[] beforePencilControlPoints;
        private bool usedPencil = false;

        public void PokeWithPencil()
        {
            usedPencil = true;
        }

        public CurveDrawMode CurveDrawMode
        {
            get
            {
                return curveDrawMode;
            }
            set
            {
                if (value == CurveDrawMode.Pencil)
                {
                    beforePencilControlPoints = controlPoints;
                    usedPencil = false;
                    controlPoints = new SortedList<int, int>[controlPoints.Length];
                    for (int c = 0; c < channels; c++)
                    {
                        controlPoints[c] = new SortedList<int, int>();
                        IList<int> xa = beforePencilControlPoints[c].Keys;
                        IList<int> ya = beforePencilControlPoints[c].Values;
                        int length = beforePencilControlPoints[c].Count;
                        switch (CurveDrawMode)
                        {
                            case CurveDrawMode.Pencil:
                            case CurveDrawMode.Linear:
                                for (int i = 0; i < length - 1; i++)
                                {
                                    for (int e = xa[i]; e <= xa[i + 1]; e++)
                                    {
                                        byte val = (byte)DoubleUtil.Lerp(ya[i], ya[i + 1], (double)(e - xa[i]) / (double)(xa[i + 1] - xa[i]));
                                        if (controlPoints[c].ContainsKey(i))
                                        {
                                            controlPoints[c][e] = val;
                                        }
                                        else
                                        {
                                            controlPoints[c].Add(e, val);
                                        }
                                    }
                                }
                                break;
                            case CurveDrawMode.Spline:
                                SplineInterpolator interpolator = new SplineInterpolator();

                                for (int i = 0; i < length; ++i)
                                {
                                    interpolator.Add(xa[i], ya[i]);
                                }

                                for (int i = 0; i < entries; ++i)
                                {
                                    byte val = interpolator.Interpolate(i).ClampToByte();
                                    if (controlPoints[c].ContainsKey(i))
                                    {
                                        controlPoints[c][i] = val;
                                    }
                                    else
                                    {
                                        controlPoints[c].Add(i, val);
                                    }
                                }
                                break;
                        }
                    }
                }
                else if (CurveDrawMode == CurveDrawMode.Pencil)
                {
                    if (usedPencil)
                    {
                        SimplifyCurve(value);
                    }
                    else
                    {
                        //no changes were made? restore.
                        if (beforePencilControlPoints != null)
                            controlPoints = beforePencilControlPoints;
                    }
                }
                curveDrawMode = value;
            }
        }

        public abstract ChannelMode ColorTransferMode
        {
            get;
        }


        protected SortedList<int, int>[] controlPoints;
        public SortedList<int, int>[] ControlPoints
        {
            get
            {
                return this.controlPoints;
            }

            set
            {
                if (value.Length != controlPoints.Length)
                {
                    throw new ArgumentException("value must have a matching channel count", "value");
                }

                this.controlPoints = value;
                Invalidate();
            }
        }

        protected int channels;
        public int Channels
        {
            get
            {
                return this.channels;
            }
        }

        protected int entries;
        public int Entries
        {
            get
            {
                return entries;
            }
        }

        protected ColorBgra[] visualColors;
        public ColorBgra GetVisualColor(int channel)
        {
            return visualColors[channel];
        }

        protected string[] channelNames;
        public string GetChannelName(int channel)
        {
            return channelNames[channel];
        }

        protected bool[] mask;
        public void SetSelected(int channel, bool val)
        {
            mask[channel] = val;
            Invalidate();
        }

        public bool GetSelected(int channel)
        {
            return mask[channel];
        }

        protected internal CurveControl(int channels, int entries)
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            this.channels = channels;
            this.entries = entries;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            pointsNearMousePerChannel = new int[channels];
            for (int i = 0; i < channels; ++i)
            {
                pointsNearMousePerChannel[i] = -1;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TabStop = false;
        }

        #endregion

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs<Point>> CoordinatesChanged;
        protected virtual void OnCoordinatesChanged()
        {
            if (CoordinatesChanged != null)
            {
                CoordinatesChanged(this, new EventArgs<Point>(new Point(lastKey, lastValue)));
            }
        }

        public void ResetControlPoints()
        {
            controlPoints = new SortedList<int, int>[Channels];

            for (int i = 0; i < Channels; ++i)
            {
                SortedList<int, int> newList = new SortedList<int, int>();

                newList.Add(0, 0);
                newList.Add(Entries - 1, Entries - 1);

                controlPoints[i] = newList;
            }
            CurveDrawMode = CurveDrawMode;

            Invalidate();
            OnValueChanged();
        }

        private void DrawToGraphics(Graphics g)
        {
            ColorBgra colorSolid = ColorBgra.FromColor(this.ForeColor);
            ColorBgra colorGuide = ColorBgra.FromColor(this.ForeColor);
            ColorBgra colorGrid = ColorBgra.FromColor(this.ForeColor);

            colorGrid.A = 128;
            colorGuide.A = 96;

            Pen penSolid = new Pen(colorSolid.ToColor(), 1);
            Pen penGrid = new Pen(colorGrid.ToColor(), 1);
            Pen penGuide = new Pen(colorGuide.ToColor(), 1);

            penGrid.DashStyle = DashStyle.Dash;

            g.Clear(this.BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle ourRect = ClientRectangle;

            ourRect.Inflate(-1, -1);

            if (lastMouseXY.Y >= 0)
            {
                g.DrawLine(penGuide, 0, lastMouseXY.Y, Width, lastMouseXY.Y);
            }

            if (lastMouseXY.X >= 0)
            {
                g.DrawLine(penGuide, lastMouseXY.X, 0, lastMouseXY.X, Height);
            }

            for (float f = 0.25f; f <= 0.75f; f += 0.25f)
            {
                float x = FloatUtil.Lerp(ourRect.Left, ourRect.Right, f);
                float y = FloatUtil.Lerp(ourRect.Top, ourRect.Bottom, f);

                g.DrawLine(penGrid,
                    Point.Round(new PointF(x, ourRect.Top)),
                    Point.Round(new PointF(x, ourRect.Bottom)));

                g.DrawLine(penGrid,
                    Point.Round(new PointF(ourRect.Left, y)),
                    Point.Round(new PointF(ourRect.Right, y)));
            }

            g.DrawLine(penGrid, ourRect.Left, ourRect.Bottom, ourRect.Right, ourRect.Top);

            float width = this.ClientRectangle.Width;
            float height = this.ClientRectangle.Height;

            for (int c = 0; c < channels; ++c)
            {
                SortedList<int, int> channelControlPoints = controlPoints[c];
                int points = channelControlPoints.Count;

                ColorBgra color = GetVisualColor(c);
                ColorBgra colorSelected = ColorBgra.Blend(color, ColorBgra.White, 128);
                Brush brush = GetBrush(color.ToColor(), c, ClientRectangle, LinearGradientMode.Vertical);

                const float penWidthNonSelected = 1;
                const float penWidthSelected = 2;
                float penWidth = mask[c] ? penWidthSelected : penWidthNonSelected;
                Pen penSelected = new Pen(brush, penWidth);

                color.A = 128;

                brush = GetBrush(color.ToColor(), c, ClientRectangle, LinearGradientMode.Vertical);
                Pen pen = new Pen(brush, penWidth);
                SolidBrush brushSelected = new SolidBrush(Color.White);

                IList<int> xa = channelControlPoints.Keys;
                IList<int> ya = channelControlPoints.Values;
                PointF[] line= new PointF[Entries];
                switch (CurveDrawMode)
                {
                    case CurveDrawMode.Spline:
                        SplineInterpolator interpolator = new SplineInterpolator();

                        for (int i = 0; i < points; ++i)
                        {
                            interpolator.Add(xa[i], ya[i]);
                        }

                        for (int i = 0; i < line.Length; ++i)
                        {
                            line[i].X = (float)i * (width - 1) / (entries - 1);
                            line[i].Y = (float)(DoubleUtil.Clamp(entries - 1 - interpolator.Interpolate(i), 0, entries - 1)) *
                                (height - 1) / (entries - 1);
                        }
                        break;
                    case CurveDrawMode.Linear:
                        for (int i = 0; i < points - 1; i++)
                        {
                            for (int e = xa[i]; e <= xa[i + 1]; e++)
                            {
                                line[e].X = (float)e * (width - 1) / (entries - 1);
                                line[e].Y = (float)(entries - 1 - DoubleUtil.Lerp(ya[i], ya[i + 1], (double)(e - xa[i]) / (double)(xa[i + 1] - xa[i]))) * (height - 1) / (entries - 1);
                            }
                        }
                        break;
                    case CurveDrawMode.Pencil:
                        for (int i = 0; i < line.Length; i++)
                        {
                            line[i].X = (float)i * (width - 1) / (entries - 1);
                            line[i].Y = (float)(entries - 1 - ya[i]) * (height - 1) / (entries - 1);
                        }
                        break;
                }

                pen.LineJoin = LineJoin.Round;
                g.DrawLines(pen, line);

                if (CurveDrawMode != CurveDrawMode.Pencil)
                {
                    for (int i = 0; i < points; ++i)
                    {
                        int k = channelControlPoints.Keys[i];
                        float x = k * (width - 1) / (entries - 1);
                        float y = (entries - 1 - channelControlPoints.Values[i]) * (height - 1) / (entries - 1);

                        const float radiusSelected = 4;
                        const float radiusNotSelected = 3;
                        const float radiusUnMasked = 2;

                        bool selected = (mask[c] && pointsNearMousePerChannel[c] == i);
                        float size = selected ? radiusSelected : (mask[c] ? radiusNotSelected : radiusUnMasked);
                        RectangleF rect = CommonUtil.SquareFromCenter(x, y, size);

                        g.FillEllipse(selected ? brushSelected : brush, rect.X, rect.Y, rect.Width, rect.Height);
                        g.DrawEllipse(selected ? penSelected : pen, rect.X, rect.Y, rect.Width, rect.Height);
                    }
                }

                pen.Dispose();
            }

            penSolid.Dispose();
            penGrid.Dispose();
            penGuide.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawToGraphics(e.Graphics);
            RenderHistogram(e.Graphics);
            base.OnPaint(e);
        }

        public virtual Brush GetBrush(Color color, int channel, Rectangle rect, LinearGradientMode lgm)
        {
            return new SolidBrush(color);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (CurveDrawMode == CurveDrawMode.Pencil
                && e.Button == MouseButtons.Right)
            {
                SmoothCurve();
            }
            else
            {
                float width = this.ClientRectangle.Width;
                float height = this.ClientRectangle.Height;
                int mx = (int)FloatUtil.Clamp(0.5f + e.X * (entries - 1) / (width - 1), 0, Entries - 1);
                int my = (int)FloatUtil.Clamp(0.5f + Entries - 1 - e.Y * (entries - 1) / (height - 1), 0, Entries - 1);

                ptSave = new Point[channels];
                for (int i = 0; i < channels; ++i)
                {
                    ptSave[i].X = -1;
                }

                if (0 != e.Button)
                {
                    tracking = (e.Button == MouseButtons.Left);
                    lastKey = mx;

                    bool anyNearMouse = false;

                    effectChannel = new bool[channels];
                    for (int c = 0; c < channels; ++c)
                    {
                        SortedList<int, int> channelControlPoints = controlPoints[c];
                        int index = pointsNearMousePerChannel[c];
                        bool hasPoint = (index >= 0);
                        int key = hasPoint ? channelControlPoints.Keys[index] : index;

                        anyNearMouse = (anyNearMouse || hasPoint);

                        effectChannel[c] = hasPoint;

                        if (mask[c] && hasPoint &&
                            key > 0 && key < entries - 1
                            && CurveDrawMode != CurveDrawMode.Pencil)
                        {
                            channelControlPoints.RemoveAt(index);
                            OnValueChanged();
                        }
                    }

                    if (!anyNearMouse)
                    {
                        for (int c = 0; c < channels; ++c)
                        {
                            effectChannel[c] = true;
                        }
                    }
                }

                OnMouseMove(e);
            }
        }

        public void SmoothCurve()
        {
            if (CurveDrawMode == CurveDrawMode.Pencil)
            {
                usedPencil = true;
            }

            for (int c = 0; c < channels; c++)
            {
                for (int i = 1; i < controlPoints[c].Count - 1; i++)
                {
                    // the value the point would be at if the curve were just a straight line from the 2 end points
                    float straight = FloatUtil.Lerp(controlPoints[c].Values[0], controlPoints[c].Values[controlPoints[c].Count - 1], controlPoints[c].Keys[i] / (float)(entries - 1));

                    int current = controlPoints[c].Values[i];

                    int newval = (int)(current + (straight - current) * .25f + .25f);

                    if (newval == current)
                        newval = (int)(0.5f + straight);

                    controlPoints[c][controlPoints[c].Keys[i]] = newval;
                }
            }
        }

        public void SimplifyCurve(CurveDrawMode drawMode)
        {
            using (new WaitCursorChanger(this.TopLevelControl != null ? this.TopLevelControl : this))
            {
                switch (drawMode)
                {
                    case CurveDrawMode.Linear:
                        SimplifyCurve(50, drawMode);
                        break;
                    case CurveDrawMode.Spline:
                        SimplifyCurve(200, drawMode);
                        break;
                }
            }
        }

        private void SimplifyCurve(int maxdelta, CurveDrawMode mode)
        {
            //SortedList<int, int>[] newControlPoints = new SortedList<int, int>[channels];
            CurveComparison comparison;
            switch (mode)
            {
                case CurveDrawMode.Linear:
                    comparison = LinearCurveComparison;
                    break;
                case CurveDrawMode.Spline:
                    comparison = SplineCurveComparison;
                    break;
                default:
                    throw new ArgumentException("mode");
            }

            for (int c = 0; c < channels; c++)
            {
                List<Point> listpoints = new List<Point>(entries);
                CopySortedListToPointList(controlPoints[c], listpoints);
                int[] curve = GetFilledCurve(controlPoints[c], mode);
                while (true)
                {
                    //find the cheapest point to discard
                    int index = -1;
                    uint cost = uint.MaxValue;
                    for (int i = 1; i < listpoints.Count - 1; i++)
                    {
                        Point val = listpoints[i];
                        listpoints.RemoveAt(i);
                        uint newcost = comparison(curve, listpoints, i - 1, 2);
                        if (newcost < cost)
                        {
                            index = i;
                            cost = newcost;
                        }
                        listpoints.Insert(i, val);
                    }
                    //if we've exceeded our quota or run out of points, quit
                    if (cost > maxdelta) break;
                    //otherwise, remove the point
                    listpoints.RemoveAt(index);
                }
                CopyPointListToSortedList(listpoints, controlPoints[c]);
            }
        }

        private void CopyPointListToSortedList(List<Point> pointList, SortedList<int, int> sortedList)
        {
            sortedList.Clear();
            for (int i = 0; i < pointList.Count; i++)
            {
                sortedList.Add(pointList[i].X, pointList[i].Y);
            }
        }

        private void CopySortedListToPointList(SortedList<int, int> sortedList, List<Point> pointList)
        {
            pointList.Clear();
            for (int i = 0; i < sortedList.Count; i++)
            {
                pointList.Add(new Point(sortedList.Keys[i], sortedList.Values[i]));
            }
        }

        private delegate uint CurveComparison(int[] curve1, List<Point> curve2, int startIndex, int length);

        private unsafe uint LinearCurveComparison(int[] curve1, List<Point> curve2, int startIndex, int length)
        {
            uint sum = 0;

            Point[] curve2array = curve2.ToArray();

            fixed (int* cp = &curve1[0]) fixed (Point* pp = &curve2array[0])
            {
                Point* c2 = pp+startIndex;
                int* c1 = cp + c2->X;
                for (int i = startIndex; i < startIndex + length - 1; i++)
                {
                    for (int e = c2->X; e < (c2 + 1)->X; e++)
                    {
                        sum += (uint)Math.Abs(*c1 - (int)FloatUtil.Lerp(c2->Y, (c2 + 1)->Y, (float)(e - c2->X) / (float)((c2 + 1)->X - c2->X)));
                        c1++;
                    }
                    c2++;
                }
            }
            return sum;
        }

        private unsafe uint SplineCurveComparison(int[] curve1, List<Point> curve2, int startIndex, int length)
        {
            uint sum = 0;

            Point[] curve2array = curve2.ToArray();

            fixed (int* cp = &curve1[0]) fixed (Point* pp = &curve2array[0])
            {
                Point* c2 = pp;
                int* c1 = cp;
                SplineInterpolator interpolator = new SplineInterpolator();
                for (int i = 0; i < curve2.Count; i++)
                {
                    interpolator.Add(c2->X, c2->Y);
                    c2++;
                }
                c1 = cp + (pp + startIndex)->X;
                for (int i = (pp + startIndex)->X; i < (pp + startIndex + length - 1)->X; i++)
                {
                    sum += (uint)Math.Abs(*c1 - interpolator.Interpolate(i).ClampToByte());
                    c1++;
                }
            }
            return sum;
        }

        private int[] GetFilledCurve(SortedList<int, int> curve, CurveDrawMode mode)
        {
            int[] ret = new int[curve.Keys[curve.Count - 1] + 1];
            IList<int> xa = curve.Keys;
            IList<int> ya = curve.Values;
            int length = curve.Count;
            switch (mode)
            {
                case CurveDrawMode.Spline:
                    SplineInterpolator interpolator = new SplineInterpolator();

                    for (int i = 0; i < length; ++i)
                    {
                        interpolator.Add(xa[i], ya[i]);
                    }

                    for (int i = 0; i < entries; ++i)
                    {
                        ret[i] = interpolator.Interpolate(i).ClampToByte();
                    }
                    break;
                case CurveDrawMode.Linear:
                    for (int i = 0; i < length - 1; i++)
                    {
                        for (int e = xa[i]; e <= xa[i + 1]; e++)
                        {
                            ret[e] = (byte)DoubleUtil.Lerp(ya[i], ya[i + 1], (double)(e - xa[i]) / (double)(xa[i + 1] - xa[i]));
                        }
                    }
                    break;
            }
            return ret;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (0 != (e.Button & MouseButtons.Left) && tracking)
            {
                tracking = false;
                lastKey = -1;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            lastMouseXY = new Point(e.X, e.Y);
            float width = this.ClientRectangle.Width;
            float height = this.ClientRectangle.Height;
            int mx = (int)FloatUtil.Clamp(0.5f + e.X * (entries - 1) / (width - 1), 0, Entries - 1);
            int my = (int)FloatUtil.Clamp(0.5f + Entries - 1 - e.Y * (entries - 1) / (height - 1), 0, Entries - 1);

            Invalidate();

            if (tracking && e.Button == MouseButtons.None)
            {
                tracking = false;
            }

            if (tracking)
            {
                bool changed = false;
                for (int c = 0; c < channels; ++c)
                {
                    SortedList<int, int> channelControlPoints = controlPoints[c];

                    pointsNearMousePerChannel[c] = -1;
                    if (mask[c] && effectChannel[c])
                    {
                        switch (CurveDrawMode)
                        {
                            case CurveDrawMode.Linear:
                            case CurveDrawMode.Spline:
                                int lastIndex = channelControlPoints.IndexOfKey(lastKey);

                                if (ptSave[c].X >= 0 && ptSave[c].X != mx)
                                {
                                    channelControlPoints[ptSave[c].X] = ptSave[c].Y;
                                    ptSave[c].X = -1;

                                    changed = true;
                                }
                                else if (lastKey > 0 && lastKey < Entries - 1 && lastIndex >= 0 && mx != lastKey)
                                {
                                    channelControlPoints.RemoveAt(lastIndex);
                                }

                                if (mx >= 0 && mx < Entries)
                                {
                                    int newValue = my.Clamp(0, Entries - 1);
                                    int oldIndex = channelControlPoints.IndexOfKey(mx);
                                    int oldValue = (oldIndex >= 0) ? channelControlPoints.Values[oldIndex] : -1;

                                    if (oldIndex >= 0 && mx != lastKey)
                                    {
                                        // if we drag onto an existing point, delete it, but save it in case we drag away
                                        ptSave[c].X = mx;
                                        ptSave[c].Y = channelControlPoints.Values[oldIndex];
                                    }

                                    if (oldIndex < 0 ||
                                        channelControlPoints[mx] != newValue)
                                    {
                                        channelControlPoints[mx] = newValue;
                                        changed = true;
                                    }

                                    pointsNearMousePerChannel[c] = channelControlPoints.IndexOfKey(mx);
                                }
                                break;
                            case CurveDrawMode.Pencil:
                                int xfrom = Math.Min(mx, lastKey);
                                int xto = Math.Max(mx, lastKey);
                                int yfrom = xfrom == mx ? my : lastValue;
                                int yto = xto == mx ? my : lastValue;
                                usedPencil = true;
                                for (int i = xfrom; i <= xto; i++)
                                {
                                    int x = i;
                                    int y;
                                    if (xto == xfrom)
                                    {
                                        y = my;
                                    }
                                    else
                                    {
                                        y = (int)FloatUtil.Lerp(yfrom, yto, (i - xfrom) / (float)(xto - xfrom));
                                    }
                                    if (channelControlPoints.ContainsKey(x))
                                    {
                                        channelControlPoints[x] = y;
                                    }
                                    else
                                    {
                                        channelControlPoints.Add(x, y);
                                    }
                                }

                                changed = true;
                                break;
                        }
                    }
                }

                if (changed)
                {
                    Update();
                    OnValueChanged();
                }
            }
            else
            {
                pointsNearMousePerChannel = new int[channels];

                for (int c = 0; c < channels; ++c)
                {
                    SortedList<int, int> channelControlPoints = controlPoints[c];
                    int minRadiusSq = 30;
                    int bestIndex = -1;

                    if (mask[c])
                    {
                        for (int i = 0; i < channelControlPoints.Count; ++i)
                        {
                            int sumsq = 0;
                            int diff = 0;

                            diff = channelControlPoints.Keys[i] - mx;
                            sumsq += diff * diff;

                            diff = channelControlPoints.Values[i] - my;
                            sumsq += diff * diff;

                            if (sumsq < minRadiusSq)
                            {
                                minRadiusSq = sumsq;
                                bestIndex = i;
                            }
                        }
                    }

                    pointsNearMousePerChannel[c] = bestIndex;
                }

                Update();
            }

            lastKey = mx;
            lastValue = my;
            OnCoordinatesChanged();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            lastKey = -1;
            lastValue = -1;
            lastMouseXY = new Point(int.MinValue, int.MinValue);
            Invalidate();
            OnCoordinatesChanged();
            base.OnMouseLeave(e);
        }

        public virtual void InitFromPixelOp(UnaryPixelOp op)
        {
            OnValueChanged();
            Invalidate();
        }

        private Histograms.Histogram histogram;
        public Histograms.Histogram Histogram
        {
            get
            {
                return histogram;
            }

            set
            {
                if (histogram != value)
                {
                    histogram = value;

                    if (value != null)
                    {
                        int channels = histogram.Channels;

                        if (mask == null || channels != mask.GetLength(0))
                        {
                            mask = new bool[channels];
                        }

                        Invalidate();
                    }
                }
            }
        }

        private void RenderChannel(Graphics g, ColorBgra color, int channel, long max)
        {
            Rectangle rect = ClientRectangle;

            int l = rect.Left;
            int t = rect.Top;
            int b = rect.Bottom;
            int r = rect.Right;
            int channels = histogram.Channels;
            int entries = histogram.Entries;
            long[] hist = histogram.HistogramValues[channel];

            PointF[] points = new PointF[entries + 2];
            points[0] = new PointF(0.0f, rect.Height);
            points[entries + 1] = new PointF(rect.Width,rect.Height);
            for (int i = 0; i < entries; i++)
            {
                points[i + 1] = new PointF(i * rect.Width / entries, rect.Height - hist[i] * rect.Height / max);
            }

            byte intensity = mask[channel] ? (byte)55 : (byte)15;
            ColorBgra colorBrush = color;

            colorBrush.A = intensity;

            Brush brush = GetBrush(colorBrush.ToColor(), channel, ClientRectangle, LinearGradientMode.Horizontal);

            g.FillPolygon(brush, points, FillMode.Alternate);
        }

        private void RenderHistogram(Graphics g)
        {
            if (histogram != null)
            {
                long max = histogram.GetMax();

                for (int i = 0; i < histogram.Channels; ++i)
                {
                    RenderChannel(g, GetVisualColor(i), i, max);
                }
            }
        }
    }
}
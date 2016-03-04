using pyrochild.effects.common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace pyrochild.effects.curvesplus
{
    public class CurvesPlusXMLFile : ICloneable
    {
        public static CurvesPlusXMLFile CreateDefault()
        {
            SortedList<int, int> list = new SortedList<int, int>();
            list.Add(0, 0);
            list.Add(255, 255);
            return new CurvesPlusXMLFile(ChannelMode.L, Channel.A, Channel.A, CurveDrawMode.Spline, new SortedList<int, int>[] { list });
        }

        public CurvesPlusXMLFile() { }

        public CurvesPlusXMLFile(ChannelMode tmm, Channel acmin, Channel acmout, CurveDrawMode cdm, SortedList<int, int>[] points)
            : this(tmm, acmin, acmout, cdm, points, "Default") { }

        private CurvesPlusXMLFile(ChannelMode tmm, Channel acmin, Channel acmout, CurveDrawMode cdm, SortedList<int, int>[] points, string name)
        {
            CurveDrawMode = cdm;
            CurveMode = tmm;
            AdvancedCurveIn = acmin;
            AdvancedCurveOut = acmout;
            ControlPoints = new Point[points.Length][];
            this.name = name;

            for (int c = 0; c < points.Length; c++)
            {
                ControlPoints[c] = new Point[points[c].Count];

                for (int i = 0; i < points[c].Count; i++)
                {
                    ControlPoints[c][i] = new Point(points[c].Keys[i], points[c].Values[i]);
                }
            }
        }

        public ChannelMode CurveMode;
        public Channel AdvancedCurveIn;
        public Channel AdvancedCurveOut;
        public CurveDrawMode CurveDrawMode;
        public Point[][] ControlPoints;

        [XmlIgnore]
        private string name;

        [XmlIgnore]
        public SortedList<int, int>[] pointlist
        {
            get
            {
                SortedList<int, int>[] list = new SortedList<int, int>[ControlPoints.Length];
                for (int c = 0; c < ControlPoints.Length; c++)
                {
                    list[c] = new SortedList<int, int>();
                    for (int i = 0; i < ControlPoints[c].Length; i++)
                    {
                        list[c].Add(ControlPoints[c][i].X, ControlPoints[c][i].Y);
                    }
                }
                return list;
            }
        }

        public ConfigToken ToConfigToken()
        {
            ConfigToken retval = new ConfigToken();
            retval.ColorTransferMode = CurveMode;
            retval.ControlPoints = pointlist;
            retval.CurveDrawMode = CurveDrawMode;
            retval.InputMode = AdvancedCurveIn;
            retval.OutputMode = AdvancedCurveOut;
            retval.Preset = name;
            return retval;
        }

        public static CurvesPlusXMLFile FromConfigToken(ConfigToken token)
        {
            return new CurvesPlusXMLFile(
                token.ColorTransferMode,
                token.InputMode,
                token.OutputMode,
                token.CurveDrawMode,
                token.ControlPoints,
                token.Preset);
        }

        #region ICloneable Members

        public object Clone()
        {
            return new CurvesPlusXMLFile(
                this.CurveMode,
                this.AdvancedCurveIn,
                this.AdvancedCurveOut,
                this.CurveDrawMode,
                this.pointlist,
                this.name);
        }

        #endregion
    }
}
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pyrochild.effects.common
{
    public sealed class SplineInterpolator
    {
        // Fields
        private SortedList<double, double> points = new SortedList<double, double>();
        private double[] y2;

        // Methods
        public void Add(double x, double y)
        {
            this.points[x] = y;
            this.y2 = null;
        }

        public void Clear()
        {
            this.points.Clear();
        }

        public double Interpolate(double x)
        {
            if (this.y2 == null)
            {
                this.PreCompute();
            }
            IList<double> xa = this.points.Keys;
            IList<double> ya = this.points.Values;
            int n = ya.Count;
            int klo = 0;
            int khi = n - 1;
            while ((khi - klo) > 1)
            {
                int k = (khi + klo) >> 1;
                if (xa[k] > x)
                {
                    khi = k;
                }
                else
                {
                    klo = k;
                }
            }
            double h = xa[khi] - xa[klo];
            double a = (xa[khi] - x) / h;
            double b = (x - xa[klo]) / h;
            return (((a * ya[klo]) + (b * ya[khi])) + (((((((a * a) * a) - a) * this.y2[klo]) + ((((b * b) * b) - b) * this.y2[khi])) * (h * h)) / 6.0));
        }

        private void PreCompute()
        {
            int n = this.points.Count;
            double[] u = new double[n];
            IList<double> xa = this.points.Keys;
            IList<double> ya = this.points.Values;
            this.y2 = new double[n];
            u[0] = 0.0;
            this.y2[0] = 0.0;
            for (int i = 1; i < (n - 1); i++)
            {
                double wx = xa[i + 1] - xa[i - 1];
                double sig = (xa[i] - xa[i - 1]) / wx;
                double p = (sig * this.y2[i - 1]) + 2.0;
                this.y2[i] = (sig - 1.0) / p;
                double ddydx = ((ya[i + 1] - ya[i]) / (xa[i + 1] - xa[i])) - ((ya[i] - ya[i - 1]) / (xa[i] - xa[i - 1]));
                u[i] = (((6.0 * ddydx) / wx) - (sig * u[i - 1])) / p;
            }
            this.y2[n - 1] = 0.0;
            for (int i = n - 2; i >= 0; i--)
            {
                this.y2[i] = (this.y2[i] * this.y2[i + 1]) + u[i];
            }
        }

        // Properties
        public int Count
        {
            get
            {
                return this.points.Count;
            }
        }
    }
}

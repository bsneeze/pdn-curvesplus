using System;
using PaintDotNet;

namespace pyrochild.effects.common
{
    [Serializable]
    public class ColorPlus
    {
        private byte a, r, g, b;
        private byte? c, m, y, k, h, s, v, l;

        public byte A
        {
            get { return a; }
            set { a = value; }
        }

        public byte R
        {
            get { return r; }
            set { r = value; }
        }

        public byte G
        {
            get { return g; }
            set { g = value; }
        }

        public byte B
        {
            get { return b; }
            set { b = value; }
        }

        public byte C
        {
            get
            {
                if (c == null)
                {
                    c = (byte?)(255 - this.R - this.K);
                }
                return (byte)c;
            }
            set
            {
                this.R = (byte)(255 - value + this.K);
                OnValueChanged();
                c = value;
            }
        }

        public byte M
        {
            get
            {
                if (m == null)
                {
                    m = (byte?)(255 - this.G - this.K);
                }
                return (byte)m;
            }
            set
            {
                this.G = (byte)(255 - value + this.K);
                OnValueChanged();
                m = value;
            }
        }

        public byte Y
        {
            get
            {
                if (y == null)
                {
                    y = (byte?)(255 - this.B - this.K);
                }
                return (byte)y;
            }
            set
            {
                this.B = (byte)(255 - value + this.K);
                OnValueChanged();
                y = value;
            }
        }

        public byte K
        {
            get
            {
                if (k == null)
                {
                    k = (byte?)Math.Min(Math.Min(255 - this.R, 255 - this.G), 255 - this.B);
                }
                return (byte)k;
            }
            set
            {
                byte oldk = Math.Max(Math.Max(this.R, this.G), this.B);
                this.R = (byte)(this.R + oldk - value);
                this.G = (byte)(this.G + oldk - value);
                this.B = (byte)(this.B + oldk - value);
                OnValueChanged();
                k = value;
            }
        }

        public byte H
        {
            get
            {
                if (h == null)
                {
                    int localh;

                    int max = (R > G) ? R : G;
                    if (max < B) max = B;

                    if (max == 0)
                    {
                        h = 0;
                        s = 0;
                        v = 0;
                    }
                    else
                    {
                        v = (byte?)max;

                        int min = (R < G) ? R : G;
                        if (min > B) min = B;

                        int delta = max - min;

                        if (delta == 0)
                        {
                            // R, G, and B must all the same.
                            // In this case, S is 0, and H is undefined.
                            // Using H = 0 is as good as any...
                            s = 0;
                            h = 0;
                        }
                        else
                        {
                            s = (byte?)CommonUtil.IntDiv(255 * delta, max);

                            if (R == max)
                            {
                                // Between Yellow and Magenta
                                localh = CommonUtil.IntDiv(255 * (G - B), delta);
                            }
                            else if (G == max)
                            {
                                // Between Cyan and Yellow
                                localh = 512 + CommonUtil.IntDiv(255 * (B - R), delta);
                            }
                            else
                            {
                                // Between Magenta and Cyan
                                localh = 1024 + CommonUtil.IntDiv(255 * (R - G), delta);
                            }

                            if (h < 0)
                            {
                                localh += 1536;
                            }

                            h = (byte?)CommonUtil.IntDiv(localh, 6);
                        }
                    }
                }
                return (byte)h;
            }
            set
            {
                int localh = value * 6;
                int locals = this.S;
                int localv = this.V;
                int tmp;

                int fSector = (localh & 0xff);
                int sNumber = (localh >> 8);
                switch (sNumber)
                {
                    case 0:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.G = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.R = (byte)localv;
                        break;
                    case 1:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.G = (byte)localv;
                        this.R = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        break;
                    case 2:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.G = (byte)localv;
                        this.R = (byte)(((localv * (255 - s)) + 128) >> 8);
                        break;
                    case 3:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)localv;
                        this.G = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.R = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        break;
                    case 4:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)localv;
                        this.G = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.R = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        break;
                    case 5:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.G = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.R = (byte)localv;
                        break;
                }
                OnValueChanged();
                v = value;
            }
        }

        public byte S
        {
            get
            {
                if (s == null)
                {
                    int max = (R > G) ? R : G;
                    if (max < B) max = B;

                    if (max == 0)
                    {
                        h = 0;
                        s = 0;
                        v = 0;
                    }
                    else
                    {
                        v = (byte?)max;

                        int min = (R < G) ? R : G;
                        if (min > B) min = B;

                        int delta = max - min;

                        if (delta == 0)
                        {
                            // R, G, and B must all the same.
                            // In this case, S is 0, and H is undefined.
                            // Using H = 0 is as good as any...
                            h = 0;
                            s = 0;
                        }
                        else
                        {
                            s = (byte?)CommonUtil.IntDiv(255 * delta, max);
                        }
                    }
                }
                return (byte)s;
            }
            set
            {
                int localh = this.H * 6;
                int locals = value;
                int localv = this.V;
                int tmp;

                int fSector = (localh & 0xff);
                int sNumber = (localh >> 8);
                switch (sNumber)
                {
                    case 0:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.G = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.R = (byte)localv;
                        break;
                    case 1:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.G = (byte)localv;
                        this.R = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        break;
                    case 2:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.G = (byte)localv;
                        this.R = (byte)(((localv * (255 - s)) + 128) >> 8);
                        break;
                    case 3:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)localv;
                        this.G = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.R = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        break;
                    case 4:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)localv;
                        this.G = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.R = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        break;
                    case 5:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.G = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.R = (byte)localv;
                        break;
                }
                OnValueChanged();
                v = value;
            }
        }

        public byte V
        {
            get
            {
                if (v == null)
                {
                    int max = (R > G) ? R : G;
                    if (max < B) max = B;

                    if (max == 0)
                    {
                        h = 0;
                        s = 0;
                        v = 0;
                    }
                    else
                    {
                        v = (byte?)max;
                    }
                }
                return (byte)v;
            }
            set
            {
                int localh = this.H * 6;
                int locals = this.S;
                int localv = value;
                int tmp;

                int fSector = (localh & 0xff);
                int sNumber = (localh >> 8);
                switch (sNumber)
                {
                    case 0:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.G = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.R = (byte)localv;
                        break;
                    case 1:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.G = (byte)localv;
                        this.R = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        break;
                    case 2:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.G = (byte)localv;
                        this.R = (byte)(((localv * (255 - s)) + 128) >> 8);
                        break;
                    case 3:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)localv;
                        this.G = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.R = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        break;
                    case 4:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)localv;
                        this.G = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.R = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        break;
                    case 5:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.G = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.R = (byte)localv;
                        break;
                }
                OnValueChanged();
                v = value;
            }
        }


        public byte L
        {
            get
            {
                if (l == null)
                {
                    l = (byte?)((7471 * this.B + 38470 * this.G + 19595 * this.R) >> 16);
                }
                return (byte)l;
            }
            set
            {
                byte oldl = (byte)((7471 * this.B + 38470 * this.G + 19595 * this.R) >> 16);
                this.R = (byte)(this.R - oldl + value);
                this.G = (byte)(this.G - oldl + value);
                this.B = (byte)(this.B - oldl + value);
                OnValueChanged();
                l = value;
            }
        }

        private void OnValueChanged()
        {
            c = m = y = k = h = s = v = l = null;
        }

        public ColorPlus(ColorBgra colorBgra)
        {
            this.R = colorBgra.R;
            this.G = colorBgra.G;
            this.B = colorBgra.B;
            this.A = colorBgra.A;
            //OnValueChanged;
        }

        public ColorBgra ToColorBgra()
        {
            return ColorBgra.FromBgra(b, g, r, a);
        }

        public Tuple<byte, byte, byte> HsvBytes
        {
            get
            {
                if (h == null)
                {
                    int localh;

                    int max = (R > G) ? R : G;
                    if (max < B) max = B;

                    if (max == 0)
                    {
                        h = 0;
                        s = 0;
                        v = 0;
                    }
                    else
                    {
                        v = (byte?)max;

                        int min = (R < G) ? R : G;
                        if (min > B) min = B;

                        int delta = max - min;

                        if (delta == 0)
                        {
                            // R, G, and B must all the same.
                            // In this case, S is 0, and H is undefined.
                            // Using H = 0 is as good as any...
                            s = 0;
                            h = 0;
                        }
                        else
                        {
                            s = (byte?)CommonUtil.IntDiv(255 * delta, max);

                            if (R == max)
                            {
                                // Between Yellow and Magenta
                                localh = CommonUtil.IntDiv(255 * (G - B), delta);
                            }
                            else if (G == max)
                            {
                                // Between Cyan and Yellow
                                localh = 512 + CommonUtil.IntDiv(255 * (B - R), delta);
                            }
                            else
                            {
                                // Between Magenta and Cyan
                                localh = 1024 + CommonUtil.IntDiv(255 * (R - G), delta);
                            }

                            if (h < 0)
                            {
                                localh += 1536;
                            }

                            h = (byte?)CommonUtil.IntDiv(localh, 6);
                        }
                    }
                }
                return new Tuple<byte, byte, byte>((byte)h, (byte)s, (byte)v);
            }
            set
            {
                int localh = value.Item1 * 6;
                int locals = value.Item2;
                int localv = value.Item3;
                int tmp;

                int fSector = (localh & 0xff);
                int sNumber = (localh >> 8);
                switch (sNumber)
                {
                    case 0:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.G = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.R = (byte)localv;
                        break;
                    case 1:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.G = (byte)localv;
                        this.R = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        break;
                    case 2:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.G = (byte)localv;
                        this.R = (byte)(((localv * (255 - s)) + 128) >> 8);
                        break;
                    case 3:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)localv;
                        this.G = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.R = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        break;
                    case 4:
                        tmp = ((locals * (255 - fSector)) + 128) >> 8;
                        this.B = (byte)localv;
                        this.G = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.R = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        break;
                    case 5:
                        tmp = ((locals * fSector) + 128) >> 8;
                        this.B = (byte)(((localv * (255 - tmp)) + 128) >> 8);
                        this.G = (byte)(((localv * (255 - locals)) + 128) >> 8);
                        this.R = (byte)localv;
                        break;
                }
                OnValueChanged();
            }
        }

        public Tuple<byte, byte, byte> Rgb
        {
            get
            {
                return new Tuple<byte, byte, byte>(R, G, B);
            }
            set
            {
                r = value.Item1;
                g = value.Item2;
                b = value.Item3;
                OnValueChanged();
            }
        }

        public Tuple<byte, byte, byte, byte> Cmyk
        {
            get
            {
                return new Tuple<byte, byte, byte, byte>(C, M, Y, K);
            }
            set
            {
                C = value.Item1;
                M = value.Item2;
                Y = value.Item3;
                K = value.Item4;
                OnValueChanged();
            }
        }
    }
}
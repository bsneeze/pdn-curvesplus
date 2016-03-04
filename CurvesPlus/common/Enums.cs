using System;

namespace pyrochild.effects.common
{
    public enum ChannelMode
    {
        L,
        Rgb,
        Hsv,
        Cmyk,
        A,
        Advanced
    }

    [Flags]
    public enum Channel
    {
        A = 1 << 1,
        R = 1 << 2,
        G = 1 << 3,
        B = 1 << 4,
        C = 1 << 5,
        M = 1 << 6,
        Y = 1 << 7,
        K = 1 << 8,
        H = 1 << 9,
        S = 1 << 10,
        V = 1 << 11,
        L = 1 << 12
    }
}

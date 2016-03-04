using System;

namespace pyrochild.effects.curvesplus
{
    public class EventArgs<T>:EventArgs
    {
        public EventArgs(T data)
        {
            Data = data;
        }

        public T Data;
    }
}

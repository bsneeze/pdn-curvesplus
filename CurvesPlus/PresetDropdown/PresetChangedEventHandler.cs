using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pyrochild.effects.common
{
    public delegate void PresetChangedEventHandler<T>
        (object sender, PresetChangedEventArgs<T> e);
}
using System;

namespace pyrochild.effects.common
{
	public class PresetChangedEventArgs<T> : EventArgs
	{
		public string Name;
		public T Preset;
		
		public PresetChangedEventArgs(string name, T preset)
		{
			Name = name;
			Preset = preset;
		}
	}
}
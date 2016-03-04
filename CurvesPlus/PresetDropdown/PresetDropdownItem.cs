using System;

namespace pyrochild.effects.common
{
    public class PresetDropdownItem<T>
    {
        public enum ItemType
        {
            Preset,
            Command,
            Separator
        }

        public string Name { get; set; }
        public T Preset { get; set; }
        public Action Action { get; set; }
        public ItemType Type { get; set; }

        public PresetDropdownItem(string name, T preset)
        {
            Name = name;
            Preset = preset;
            Type = ItemType.Preset;
        }

        public PresetDropdownItem(string name, Action action)
        {
            Name = name;
            Action = action;
            Type = ItemType.Command;
        }

        public PresetDropdownItem()
        {
            Type = ItemType.Separator;
        }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}
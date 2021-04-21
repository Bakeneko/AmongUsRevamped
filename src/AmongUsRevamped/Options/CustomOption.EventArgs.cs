using System;
using System.ComponentModel;

namespace AmongUsRevamped.Options
{
    public class OptionOnValueChangedEventArgs : CancelEventArgs
    {
        public object NewValue { get; set; }
        public object OldValue { get; private set; }

        public OptionOnValueChangedEventArgs(object newValue, object oldValue)
        {
            NewValue = newValue;
            OldValue = oldValue;
        }
    }

    public class ToggleOptionOnValueChangedEventArgs : OptionOnValueChangedEventArgs
    {
        public new bool NewValue { get { return (bool)base.NewValue; } set { NewValue = value; } }
        public new bool OldValue { get { return (bool)base.OldValue; } }

        public ToggleOptionOnValueChangedEventArgs(object newValue, object oldValue) : base(newValue, oldValue)
        {
        }
    }

    public class NumberOptionOnValueChangedEventArgs : OptionOnValueChangedEventArgs
    {
        public new float NewValue { get { return (float)base.NewValue; } set { NewValue = value; } }
        public new float OldValue { get { return (float)base.OldValue; } }

        public NumberOptionOnValueChangedEventArgs(object newValue, object oldValue) : base(newValue, oldValue)
        {
        }
    }

    public class StringOptionOnValueChangedEventArgs : OptionOnValueChangedEventArgs
    {
        public new int NewValue { get { return (int)base.NewValue; } set { NewValue = value; } }
        public new int OldValue { get { return (int)base.OldValue; } }

        public StringOptionOnValueChangedEventArgs(object newValue, object oldValue) : base(newValue, oldValue)
        {
        }
    }

    public class OptionValueChangedEventArgs : EventArgs
    {
        public readonly object OldValue;
        public readonly object NewValue;

        public OptionValueChangedEventArgs(object newValue, object oldValue)
        {
            NewValue = newValue;
            OldValue = oldValue;
        }
    }

    public class ToggleOptionValueChangedEventArgs : OptionValueChangedEventArgs
    {
        public new bool OldValue { get { return (bool)base.OldValue; } }
        public new bool NewValue { get { return (bool)base.NewValue; } }

        public ToggleOptionValueChangedEventArgs(object newValue, object oldValue) : base(newValue, oldValue)
        {
        }
    }

    public class NumberOptionValueChangedEventArgs : OptionValueChangedEventArgs
    {
        public new float OldValue { get { return (float)base.OldValue; } }
        public new float NewValue { get { return (float)base.NewValue; } }

        public NumberOptionValueChangedEventArgs(object newValue, object oldValue) : base(newValue, oldValue)
        {
        }
    }

    public class StringOptionValueChangedEventArgs : OptionValueChangedEventArgs
    {
        public new int OldValue { get { return (int)base.OldValue; } }
        public new int NewValue { get { return (int)base.NewValue; } }

        public StringOptionValueChangedEventArgs(object newValue, object oldValue) : base(newValue, oldValue)
        {
        }
    }
}

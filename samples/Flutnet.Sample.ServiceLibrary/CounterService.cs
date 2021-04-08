using System;
using Flutnet.ServiceModel;

namespace Flutnet.Sample.ServiceLibrary
{
    [PlatformService]
    public class CounterService
    {
        [PlatformEvent]
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        private int _value = 0;

        [PlatformOperation]
        public int GetValue()
        {
            return _value;
        }

        [PlatformOperation]
        public void Increment()
        {
            _value++;
            OnValueChanged( new ValueChangedEventArgs()
            {
                Value = _value
            });
        }

        [PlatformOperation]
        public void Decrement()
        {
            _value--;
            OnValueChanged(new ValueChangedEventArgs()
            {
                Value = _value
            });
        }

        protected virtual void OnValueChanged(ValueChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }
    }
}

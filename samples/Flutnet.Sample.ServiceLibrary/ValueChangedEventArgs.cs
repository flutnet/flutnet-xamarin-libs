using System;
using Flutnet.ServiceModel;

namespace Flutnet.Sample.ServiceLibrary
{
    [PlatformData]
    public class ValueChangedEventArgs : EventArgs
    {
        public int Value { get; set; }
    }
}

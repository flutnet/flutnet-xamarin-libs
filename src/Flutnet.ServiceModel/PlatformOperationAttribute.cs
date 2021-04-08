using System;

namespace Flutnet.ServiceModel
{
    /// <summary>
    /// Denotes a method that can be invoked from Flutter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PlatformOperationAttribute : Attribute
    {
        /// <summary>
        /// Indicates whether the method must be invoked on the main (UI) thread
        /// when the underlying platform is Android.
        /// </summary>
        public bool AndroidMainThreadRequired { get; set; }

        /// <summary>
        /// Indicates whether the method must be invoked on the main (UI) thread
        /// when the underlying platform is iOS.
        /// </summary>
        public bool IosMainThreadRequired { get; set; }
    }
}
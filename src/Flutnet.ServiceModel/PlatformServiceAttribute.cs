using System;

namespace Flutnet.ServiceModel
{
    /// <summary>
    /// Denotes a class or an interface that exposes methods that can be invoked from Flutter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class PlatformServiceAttribute : Attribute
    {
    }
}
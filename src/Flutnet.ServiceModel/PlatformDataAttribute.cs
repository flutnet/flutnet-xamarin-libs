using System;

namespace Flutnet.ServiceModel
{
    /// <summary>
    /// Denotes a class that can be used to exchange information with Flutter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
    public sealed class PlatformDataAttribute : Attribute
    {
    }
}
using System;

namespace Flutnet.ServiceModel
{
    /// <summary>
    /// Denotes an event that is "propagated" to Flutter. The underlying delegate type must be <see cref="EventHandler"/> or <see cref="EventHandler{TEventArgs}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Event)]
    public sealed class PlatformEventAttribute : Attribute
    {
    }
}
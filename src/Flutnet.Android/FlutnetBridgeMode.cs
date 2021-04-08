namespace Flutnet
{
    /// <summary>
    /// Specifies how platform code and Flutter code communicate.
    /// </summary>
    public enum FlutnetBridgeMode
    {
        /// <summary>
        /// Communication uses standard Flutter platform channel.
        /// </summary>
        PlatformChannel,
        /// <summary>
        /// Communication uses WebSocket protocol.
        /// </summary>
        WebSocket
    }
}
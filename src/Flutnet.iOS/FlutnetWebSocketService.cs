using System;
using UIKit;

namespace Flutnet
{
    internal class FlutnetWebSocketService : IDisposable
    {
        readonly FlutnetWebSocket _socket;
        readonly nint _taskId;

        public FlutnetWebSocketService()
        {
            // Generate a long task when start the debug service
            _taskId = UIApplication.SharedApplication.BeginBackgroundTask("FlutnetWebSocket", () =>
            {
                // Expiration handler do nothing
            });

            _socket = new FlutnetWebSocket();
            _socket.Start();
        }

        public void Dispose()
        {
            _socket?.Stop();

            UIApplication.SharedApplication.EndBackgroundTask(_taskId);
        }
    }
}
// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

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
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

using Android.App;
using Android.Content;
using Android.OS;

namespace Flutnet
{
    ///  \private
    [Service]
    ///  \private
    public class FlutnetWebSocketService : Service
    {
        FlutnetWebSocket _socket;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            _socket = new FlutnetWebSocket();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        { 
            _socket.Start();

            // Keep the service open
            return StartCommandResult.NotSticky;
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            // The user kill the app from the task manager
            _socket.Stop();

            base.OnTaskRemoved(rootIntent);
        }
    }
}
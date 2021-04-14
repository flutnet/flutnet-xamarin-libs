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
using System.Reflection;
using Android.OS;

namespace Flutnet
{
    [Obfuscation(Exclude = true)]
    internal static class MainThread
    {
        static volatile Handler _handler;

        /// <summary>
        /// Returns <see langword="true"/> if code is running on the main (UI) thread.
        /// </summary>
        public static bool IsMainThread
        {
            get
            {
                if ((int) Build.VERSION.SdkInt >= (int) BuildVersionCodes.M)
                    return Looper.MainLooper.IsCurrentThread;

                return Looper.MyLooper() == Looper.MainLooper;
            }
        }

        /// <summary>
        /// Invokes an Action on the main (UI) thread.
        /// </summary>
        /// <param name="action">The Action to invoke</param>
        public static void BeginInvokeOnMainThread(Action action)
        {
            // The implementation of this method mimics Xamarin.Essentials MainThread
            // https://github.com/xamarin/Essentials/blob/master/Xamarin.Essentials/MainThread/MainThread.android.cs
            // Please note: Action code is NOT executed immediately, but is added to the Looper's message queue 
            // and is handled when the UI thread's scheduled to handle its message.
            // For further info see: https://stackoverflow.com/a/45663945
            // and: https://docs.microsoft.com/en-US/xamarin/essentials/main-thread

            if (_handler?.Looper != Looper.MainLooper)
                _handler = new Handler(Looper.MainLooper);

            Console.WriteLine("Posting action on main thread...");
            _handler.Post(action);
        }
    }
}
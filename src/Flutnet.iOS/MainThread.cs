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

using Foundation;
using System;
using System.Reflection;

namespace Flutnet
{
    [Obfuscation(Exclude = true)]
    internal static class MainThread
    {
        /// <summary>
        /// Returns <see langword="true"/> if code is running on the main (UI) thread.
        /// </summary>
        public static bool IsMainThread
        {
            get
            {
                return NSThread.Current.IsMainThread;
            }
        }

        /// <summary>
        /// Invokes an Action on the main (UI) thread.
        /// </summary>
        /// <param name="action">The Action to invoke</param>
        public static void BeginInvokeOnMainThread(Action action)
        {
            // The implementation of this method mimics Xamarin.Essentials MainThread
            // https://github.com/xamarin/Essentials/blob/master/Xamarin.Essentials/MainThread/MainThread.ios.tvos.watchos.cs
            // Please note: Action code is executed when the main thread goes back to its main loop for processing events.
            // For further info see: https://stackoverflow.com/a/45663945
            // and: https://docs.microsoft.com/en-US/xamarin/essentials/main-thread

            Console.WriteLine("Posting action on main thread...");
            NSRunLoop.Main.BeginInvokeOnMainThread(action.Invoke);
        }
    }
}
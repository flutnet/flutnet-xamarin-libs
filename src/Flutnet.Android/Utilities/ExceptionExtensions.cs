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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Flutnet.Utilities
{
    internal static class ExceptionExtensions
    {
        static readonly Assembly FlutnetAssembly = typeof(FlutnetBridge).Assembly;

        public static string GetDetails(this Exception exception)
        {

#if __ANDROID__
            // Get stack trace for the exception with source file information
            StackTrace st = new StackTrace(exception, true);

            if (st.FrameCount > 0)
            {
                // Get the top stack frame
                StackFrame frame = st.GetFrame(0);
                // Get the line number from the stack frame
                int line = frame.GetFileLineNumber();
                // Get the line number from the stack frame
                string filename = frame.GetFileName();
                // Get the line number from the stack frame
                string details = $"{exception.GetType()} thrown at line {line}, file: {filename}.\nError message: {exception.Message}";
                return details;
            }
#endif

            return exception.ToString();
        }

        /// <summary>
        /// Returns a particular representation of the stack trace for this exception
        /// where methods defined inside Flutnet class library are excluded.
        /// See: https://stackoverflow.com/questions/2973343/how-to-hide-the-current-method-from-exception-stack-trace-in-net
        /// </summary>
        public static string GetStackTraceCleared(this Exception exception)
        {

            var stacktrace = new StackTrace(exception, true);

            var frames = stacktrace.GetFrames() ?? new StackFrame[0];

            var selectedFrames = frames.Where(frame => frame.GetMethod()?.DeclaringType?.Assembly != FlutnetAssembly).ToList();

            // full stacktrace info list
            List<string> frameInfos = new List<string>();
            foreach (StackFrame frame in selectedFrames)
            {
                if (frame == null)
                    continue;

                string info = new StackTrace(frame).ToString();

                frameInfos.Add(info);
            }

            return frameInfos.Count > 0 ? string.Concat(frameInfos) : exception.GetDetails();


            /*
            return string.Concat(
                new StackTrace(exception, true)
                    .GetFrames()
                    .Where(frame => frame.GetMethod().DeclaringType?.Assembly != FlutnetAssembly)
                    // The following line is required as we want the usual stack trace formatting:
                    // StackFrame.ToString() formats differently
                    .Select(frame => new StackTrace(frame).ToString())
                    .ToArray());*/
        }

        /// <summary>
        /// Return all the exception info.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string ToStringCleared(this Exception exception)
        {
            string message = exception.Message;

            string header;

            if (message == null || message.Length <= 0)
            {
                header = exception.GetType().ToString();
            }
            else
            {
                header = exception.GetType().ToString() + ": " + message;
            }

            return $"{header}{Environment.NewLine}{exception.GetStackTraceCleared()}";
        }
    }
}
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

namespace Flutnet.ServiceModel
{
    /// <summary>
    /// Represents errors that occur during the execution of a method decorated with [PlatformOperation]. See <see cref="PlatformOperationAttribute"/>.
    /// Throwing this exception will result in an exception with the same name thrown in Flutter.
    /// </summary>
    public class PlatformOperationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformOperationException"/> class.
        /// </summary>
        public PlatformOperationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformOperationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PlatformOperationException(string message) : base(message)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformOperationException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public PlatformOperationException(string message, Exception inner) : base(message, inner)
        {
            Message = message;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message { get; }
    }
}
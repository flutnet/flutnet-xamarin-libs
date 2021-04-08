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
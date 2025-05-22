namespace BoxLib
{
    using System;

    /// <summary>
    /// Exception thrown when a Box operation fails.
    /// </summary>
    public class BoxException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoxException"/> class.
        /// </summary>
        public BoxException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public BoxException(string message)
            : base(message)
        {
        }
    }
}
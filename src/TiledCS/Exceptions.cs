using System;

namespace TiledCS
{
    /// <summary>
    /// Represents an exception only thrown by TiledCS
    /// </summary>
    public class TiledException : Exception
    {
        /// <summary>
        /// Returns an instance of TiledException
        /// </summary>
        /// <param name="message">The exception message</param>
        public TiledException(string message) : base(message)
        {
            
        }
        /// <summary>
        /// Returns an instance of TiledException
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="inner">The inner exception</param>
        public TiledException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
using System;

namespace TiledSharp
{
    public class TiledException : Exception
    {
        public TiledException(string message) : base(message)
        {
            
        }
        public TiledException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
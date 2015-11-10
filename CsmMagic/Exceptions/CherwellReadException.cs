using System;

namespace CsmMagic.Exceptions
{
    /// <summary>
    /// Describes an error that occurs when reading data from Cherwell
    /// </summary>
    public class CherwellReadException : Exception
    {
        public CherwellReadException(string message) : base(message)
        {
            
        }
    }
}

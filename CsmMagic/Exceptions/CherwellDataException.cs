using System;

namespace CsmMagic.Exceptions
{
    /// <summary>
    /// Describes an error where code expects Cherwell  to contain data or schema configurations that do not in fact exist
    /// </summary>
    public class CherwellDataException : Exception
    {
        public CherwellDataException(string message) : base(message)
        {
        }

        public CherwellDataException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}
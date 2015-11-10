using System;

namespace CsmMagic.Exceptions
{
    /// <summary>
    ///     A problem updating a Cherwell entry
    /// </summary>
    public class CherwellUpdateException : Exception
    {
        public CherwellUpdateException(string message, string errorCode, string errorText) : base(message)
        {
            ErrorCode = errorCode;
            ErrorText = errorText;
        }

        public CherwellUpdateException(string message) : base(message)
        {
        }

        public CherwellUpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string ErrorCode { get; set; }

        public string ErrorText { get; set; }
    }
}
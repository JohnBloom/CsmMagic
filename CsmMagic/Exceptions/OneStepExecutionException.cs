using System;

namespace CsmMagic.Exceptions
{
    /// <summary>
    ///     A problem executing a one step
    /// </summary>
    public class OneStepExecutionException : Exception
    {
        public OneStepExecutionException(string message, string errorCode, string errorText) : base(message)
        {
            ErrorCode = errorCode;
            ErrorText = errorText;
        }

        public OneStepExecutionException(string message) : base(message)
        {
        }

        public OneStepExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string ErrorCode { get; set; }

        public string ErrorText { get; set; }
    }
}
using System;

namespace CsmMagic.Validation
{
    public class CsmValidationAttribute : Attribute
    {
        public object Validator { get; private set; }

        public bool ThrowException { get; private set; }

        public string Message { get; private set; }

        public CsmValidationAttribute(Type validator, bool throwException = true, string message = "")
        {
            if (validator == null)
            {
                throw new ArgumentException("validator");
            }
            Validator = Activator.CreateInstance(validator);
            ThrowException = throwException;
            Message = message;
        }
    }
}

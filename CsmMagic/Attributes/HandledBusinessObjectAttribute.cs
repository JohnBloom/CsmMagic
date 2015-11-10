using System;

namespace CsmMagic.Attributes
{
    /// <summary>
    /// Notes that the decorated class should be handled by the specified type of handler.
    /// This ensures that when the CsmClient encounters the decorated type, it will delegate overridden functionality to the specified handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HandledBusinessObjectAttribute : Attribute
    {
        public HandledBusinessObjectAttribute(Type handler)
        {
            Handler = handler;
        }

        public Type Handler { get; private set; }
    }
}

using System;
using System.Runtime.CompilerServices;

namespace CsmMagic.Attributes
{
    /// <summary>
    /// Decorating a property with this attribute will allow the value of the property to be read and, optionally, written from Cherwell.
    /// The Name property of the attribute must match the internal name of the corresponding Cherwell blueprint field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        private readonly bool _isWriteable;
        private readonly string _name;

        /// <summary>
        /// Describes a property that maps to a field in a Cherwell blueprint
        /// </summary>
        /// <param name="name">The internal name of the data field</param>
        /// <param name="isWriteable">Some blueprint fields - calculated expressions, particularly - cannot be written to. You can mark properties as unwritable with this param.</param>
        public FieldAttribute([CallerMemberName] string name = null, bool isWriteable = true)
        {
            _name = name;
            _isWriteable = isWriteable;
        }

        public bool IsWriteable
        {
            get
            {
                return _isWriteable;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}
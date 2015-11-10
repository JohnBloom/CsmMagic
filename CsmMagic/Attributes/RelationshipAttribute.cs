using System;
using System.Runtime.CompilerServices;

namespace CsmMagic.Attributes
{
    /// <summary>
    /// Decorating a property with this attribute will allow the property to act as a navigational property within an object graph in Cherwell.
    /// The RelationshipName property of the attribute must match the internal name of the corresponding Relationship definition in Cherwell.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RelationshipAttribute : Attribute
    {
        public RelationshipAttribute([CallerMemberName] string name = null)
        {
            RelationshipName = name;
        }

        public string RelationshipName { get; set; }
    }
}
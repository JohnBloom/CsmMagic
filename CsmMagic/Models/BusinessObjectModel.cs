using System;
using CsmMagic.Attributes;

namespace CsmMagic.Models
{
    /// <summary>
    /// A fundamental class from which all models that interact with Cherwell should derive
    /// </summary>
    public abstract class BusinessObjectModel
    {
        protected BusinessObjectModel(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentNullException("typeName");
            }

            TypeName = typeName;
        }

        /// <summary>
        /// The Cherwell record ID of this business object
        /// </summary>
        [Key]
        [Field("RecID", false)]
        public string RecId { get; set; }

        /// <summary>
        /// The name of the business object in Cherwell
        /// </summary>
        internal string TypeName { get; set; }
    }
}
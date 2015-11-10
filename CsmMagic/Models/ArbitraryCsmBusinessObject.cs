using System.Collections.Generic;

namespace CsmMagic.Models
{
    public class ArbitraryCsmBusinessObject
    {
        public ArbitraryCsmBusinessObject(string typeName, string recId)
        {
            TypeName = typeName;
            RecId = recId;
            FieldsAndValues = new Dictionary<string, string>();
        }
        public ArbitraryCsmBusinessObject(string typeName)
        {
            TypeName = typeName;
            FieldsAndValues = new Dictionary<string, string>();
        }

        public string TypeName { get; private set; }

        public string RecId { get; internal set; }

        public Dictionary<string, string> FieldsAndValues { get; set; }
    }
}

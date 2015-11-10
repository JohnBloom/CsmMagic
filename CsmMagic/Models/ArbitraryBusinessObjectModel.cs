using System.Collections.Generic;

namespace CsmMagic.Models
{
    public class ArbitraryBusinessObjectModel
    {
        public ArbitraryBusinessObjectModel(string typeName, string recId)
        {
            TypeName = typeName;
            RecId = recId;
            FieldsAndValues = new Dictionary<string, string>();
        }

        public string TypeName { get; private set; }

        public string RecId { get; private set; }

        public Dictionary<string, string> FieldsAndValues { get; set; }
    }
}

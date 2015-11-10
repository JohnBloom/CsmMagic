using CsmMagic.Attributes;
using CsmMagic.Models;
using CsmMagic.Test.Validators;
using CsmMagic.Validation;

namespace CsmMagic.Test.Models
{
    public class TestIncidentBase : BusinessObjectModel
    {
        public TestIncidentBase()
            : base("Incident")
        {
        }


        [Field("ShortDescription")]
        [CsmValidation(typeof(IncidentTitleValidator), true, "We should not allow titles with bad information")]
        public string Title { get; set; }

        [Field]
        [CsmValidation(typeof(IncidentDescriptionValidator), false)]
        public string Description { get; set; }
    }
}

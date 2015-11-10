using CsmMagic.Attributes;
using CsmMagic.Models;
using CsmMagic.Test.Validators;
using CsmMagic.Validation;

namespace CsmMagic.Test.Models
{
    public class NullableFieldIncident : TestIncidentBase
    {
        [Field]
        public decimal? Priority { get; set; }

        [Field("IncidentDurationInHours")]
        public decimal? DurationInHours { get; set; }

        [Field("CustomerRecID")]
        public string CustomerId { get; set; }
    }
}

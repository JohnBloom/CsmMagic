using CsmMagic.Attributes;
using CsmMagic.Models;
using CsmMagic.Test.Validators;
using CsmMagic.Validation;

namespace CsmMagic.Test.Models
{
    public class TestIncident : TestIncidentBase
    {
        [Field]
        public decimal Priority { get; set; }

        [Key]
        [Field]
        public string IncidentID { get; set; }

        [Field("IncidentDurationInHours")]
        public decimal DurationInHours { get; set; }

        [Field("CustomerRecID")]
        public string CustomerId { get; set; }
    }
}

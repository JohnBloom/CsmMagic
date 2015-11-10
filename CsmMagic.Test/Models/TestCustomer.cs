using System.Collections.Generic;
using CsmMagic.Attributes;
using CsmMagic.Models;
using CsmMagic.Test.Handlers;
using CsmMagic.Test.Models;
using CsmMagic.Test.Validators;
using CsmMagic.Validation;

namespace CsmMagic.Test
{
    [HandledBusinessObject(typeof(CustomerHandler))]
    public class TestCustomer : BusinessObjectModel
    {
        //The constructor takes the name of the business object that is represented in cherwell.
        public TestCustomer()
            : base("Customer")
        {
        }

        //No need to put a RecId in your model. It is included in the base for you.
        //public string RecId { get; set; }

        //If the name of the property matches the name in Cherwell you dont need to 
        //include the name in the [Field] attribute
        [Field]
        public string Phone { get; set; }

        //If the name of the property does not match the name in Cherwell you DO need to 
        //include the name of the property in the [Field] attribute
        //The name can be the display name or the internal name in Cherwell.
        [Field("FullName"), CsmValidation(typeof(CustomerNameValidator))]
        public string Name { get; set; }

        //The relationship attribute relates and object to other objects. 
        //The name has to match the name in Cherwell and be linked to an
        //IEnumerable of the related type.
        [Relationship("CustomersHaveIncidents")]
        public IEnumerable<TestIncident> Incidents { get; set; }
        
    }
}

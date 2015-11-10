using System;
using CsmMagic.Test.Models;
using CsmMagic.Validation;

namespace CsmMagic.Test.Validators
{
    public class IncidentDescriptionValidator : CsmValidator<TestIncidentBase>
    {
        public override bool Validate(string fieldName, object value, TestIncidentBase entity)
        {
            //Returning true passes validation
            return value.ToString().Contains("Test");
        }
    }
}

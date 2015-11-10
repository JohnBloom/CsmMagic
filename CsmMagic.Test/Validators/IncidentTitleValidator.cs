using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsmMagic.Test.Models;
using CsmMagic.Validation;

namespace CsmMagic.Test.Validators
{
    public class IncidentTitleValidator : CsmValidator<TestIncidentBase>
    {
        public override bool Validate(string fieldName, object value, TestIncidentBase entity)
        {
            //Returning true passes validation
            return !value.ToString().Contains("throw exception");
        }
    }
}

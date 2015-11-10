using CsmMagic.Validation;

namespace CsmMagic.Test.Validators
{
    internal class CustomerNameValidator : CsmValidator<TestCustomer>
    {
        public override bool Validate(string fieldName, object value, TestCustomer entity)
        {
            return !value.ToString().Contains("Fail");
        }
    }
}

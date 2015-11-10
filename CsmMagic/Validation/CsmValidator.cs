using CsmMagic.Models;

namespace CsmMagic.Validation
{
    public abstract class CsmValidator<T> where T : BusinessObjectModel
    {
        public abstract bool Validate(string fieldName, object value, T entity);
    }
}

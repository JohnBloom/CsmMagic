using CsmMagic.Models;

namespace CsmMagic.Handlers
{
    internal class DefaultBusinessObjectHandler<T> : BaseBusinessObjectHandler<T> where T : BusinessObjectModel, new()
    {
    }
}

using CsmMagic.Exceptions;
using CsmMagic.Models;
using CsmMagic.Transactions;
using Trebuchet.API;

namespace CsmMagic.Handlers
{
    /// <summary>
    /// This class provides an extension point for custom CRUD (Create, Read, Update, Delete) functionality.
    /// The base implementation of these methods call the methods within the CsmClient, but they can all be overridden by clients.
    /// These providers may be used directly, or they can be registered with CsmMagic, and they will be used automatically when TBusinessObjects are acted upon.
    /// </summary>
    /// <remarks>
    /// This implementation defines the default behavior of the library. As such, the <see cref="IHandlerClient">client</see> param is unusued.
    /// Subclasses will make use of that interface to create the specific sequences of client method calls that are required for that type handler.
    /// </remarks>        
    public abstract class BaseBusinessObjectHandler<TBusinessObject> where TBusinessObject : BusinessObjectModel, new()
    {
        public virtual void Create(TBusinessObject incomingDomainObject, IHandlerClient client)
        {
            var newTrebuchetObject = TrebuchetApi.Api.BusObServices.CreateBusinessObjectByName(incomingDomainObject.TypeName);
            var transaction = new CsmMagicCreateTransaction<TBusinessObject>(incomingDomainObject, newTrebuchetObject);
            RunWriteTransaction(transaction);
        }

        public virtual void Delete(TBusinessObject entity, IHandlerClient client)
        {
            var typeName = entity.TypeName;
            var busObDef = TrebuchetApi.Api.DefinitionRepository.GetBusObDefByNameOrId(typeName);
            var result = TrebuchetApi.Api.BusObServices.DeleteBusObById(busObDef.Id, entity.RecId);

            if (!result.Success)
            {
                throw new CherwellUpdateException(result.ErrorText);
            }
        }

        public virtual void Update(TBusinessObject incomingDomainObject, IHandlerClient client)
        {
            var trebuchetRepresentation = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(incomingDomainObject.TypeName, incomingDomainObject.RecId);

            if (trebuchetRepresentation == null)
            {
                throw new RecordNotFoundException(incomingDomainObject.RecId, incomingDomainObject.TypeName);
            }

            var transaction = new CsmMagicUpdateTransaction<TBusinessObject>(incomingDomainObject, trebuchetRepresentation);
            RunWriteTransaction(transaction);
        }

        private static void RunWriteTransaction(CsmMagicWriteTransaction<TBusinessObject> transaction)
        {
            transaction.Execute();
            if (transaction.WasSuccessful)
            {
                return;
            }
            transaction.Rollback();
            throw new CherwellUpdateException("Write failed in transaction", transaction.FailException);
        }
    }
}
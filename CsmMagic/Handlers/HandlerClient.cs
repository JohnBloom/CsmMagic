using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using CsmMagic.Models;
using CsmMagic.Queries;

namespace CsmMagic.Handlers
{
    /// <summary>
    /// The handler client exists to ensure that you don't get caught in an endless loop of:
    ///     * Call write method on client
    ///     * Client defers to handler for provided type
    ///     * Handler calls write method on client
    ///     * Client defers to handler for provided type
    ///     * Handler calls write method on client [etc...]
    /// The handler client passes through to the client, but calls specific "don't defer to a handler here" methods when its generic constraint is the same as the constraint on its pass-through methods
    /// Currently this only applies to write operations (Create, Update, Delete), but we may add more handled functionality, in which case those methods will need to perform the same check.
    /// </summary>
    /// <typeparam name="TStackOverflowProtectionMarker"></typeparam>
    internal class HandlerClient<TStackOverflowProtectionMarker> : IHandlerClient where TStackOverflowProtectionMarker : BusinessObjectModel, new()
    {
        private readonly CsmClient _client;

        internal HandlerClient(CsmClient client)
        {
            _client = client;
        } 

        public void Dispose()
        {
            _client.Dispose();
        }

        public void Create<TBusinessObjectModel>(TBusinessObjectModel businessObjectModel) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            if (typeof(TStackOverflowProtectionMarker) == typeof(TBusinessObjectModel))
            {
                _client.CreateInCherwell(businessObjectModel);
                return;
            }

            _client.Create(businessObjectModel);
        }

        public void Create(ArbitraryCsmBusinessObject model)
        {
            _client.Create(model);
        }

        public void Delete<TBusinessObjectModel>(TBusinessObjectModel entity) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            if (typeof(TStackOverflowProtectionMarker) == typeof(TBusinessObjectModel))
            {
                _client.DeleteInCherwell(entity);
                return;
            }

            _client.Create(entity);
        }

        public IEnumerable<TBusinessObjectModel> ExecuteQuery<TBusinessObjectModel>(ICsmQuery<TBusinessObjectModel> query) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            return _client.ExecuteQuery(query);
        }

        public IEnumerable<TBusinessObjectModel> ExecuteQuery<TBusinessObjectModel>(ICsmQueryClause<TBusinessObjectModel> clause) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            return _client.ExecuteQuery(clause);
        }

        public ICsmArbitraryQuery GetArbitraryQuery()
        {
            return _client.GetArbitraryQuery();
        }

        public string GetBusinessObjectTypeId(string businessObjectTypeName)
        {
            return _client.GetBusinessObjectTypeId(businessObjectTypeName);
        }

        public ICsmQuery<TBusinessObjectModel> GetQuery<TBusinessObjectModel>() where TBusinessObjectModel : BusinessObjectModel, new()
        {
            return _client.GetQuery<TBusinessObjectModel>();
        }

        public void LinkChildToParent<TChild, TParent>(Expression<Func<TParent, object>> relationshipPointer, TChild related, string parentRecId) where TChild : BusinessObjectModel where TParent : BusinessObjectModel, new()
        {
            _client.LinkChildToParent(relationshipPointer, related, parentRecId);
        }

        public void LinkOneToOne<TChild, TParent>(Expression<Func<TParent, object>> relationshipPointer, TChild related, string parentRecId) where TChild : BusinessObjectModel where TParent : BusinessObjectModel, new()
        {
            _client.LinkOneToOne(relationshipPointer, related, parentRecId);
        }

        public void UnlinkChildFromParent<TChild, TParent>(Expression<Func<TParent, object>> relationshipPointer, TChild child, string parentRecId) where TChild : BusinessObjectModel where TParent : BusinessObjectModel, new()
        {
            _client.UnlinkChildFromParent(relationshipPointer, child, parentRecId);
        }

        public ArbitraryCsmBusinessObject ReadArbitraryModel(string objectName, string recId)
        {
            return _client.ReadArbitraryModel(objectName, recId);
        }

        public void Update<TBusinessObjectModel>(TBusinessObjectModel businessObject) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            if (typeof(TStackOverflowProtectionMarker) == typeof(TBusinessObjectModel))
            {
                _client.UpdateInCherwell(businessObject);
                return;
            }

            _client.Update(businessObject);
        }

        public void Update(ArbitraryCsmBusinessObject model)
        {
            _client.Update(model);
        }

        public IEnumerable<ArbitraryCsmBusinessObject> ExecuteQuery(ICsmArbitraryQuery query)
        {
            return _client.ExecuteQuery(query);
        }

        public void DeleteByTableAndRecId(string tableName, string id)
        {
            _client.DeleteByTableAndRecId(tableName, id);
        }

        public void AttachFileToObject<TBusinessObjectModel>(TBusinessObjectModel entity, IEnumerable<Attachment> paths) where TBusinessObjectModel : BusinessObjectModel
        {
            _client.AttachFileToObject(entity,paths);
        }

        public IEnumerable<Attachment> GetAttachmentsFromObject<TBusinessObjectModel>(TBusinessObjectModel entity) where TBusinessObjectModel : BusinessObjectModel
        {
            return _client.GetAttachmentsFromObject(entity);
        }

        public int HasAttachments<TBusinessObjectModel>(TBusinessObjectModel entity) where TBusinessObjectModel : BusinessObjectModel
        {
            return _client.HasAttachments(entity);
        }

        public void ExecuteOneStep(string oneStepName, BusinessObjectModel businessObjectModel)
        {
            _client.ExecuteOneStep(oneStepName, businessObjectModel);
        }

    }
}

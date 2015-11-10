using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using CsmMagic.Attributes;
using CsmMagic.Exceptions;
using CsmMagic.Handlers;
using CsmMagic.Models;
using CsmMagic.Queries;
using Trebuchet;
using Trebuchet.API;

namespace CsmMagic
{
    /// <summary>
    ///     The csm client.
    /// </summary>
    public class CsmClient : ICsmClient
    {
        private const int _maxPageSize = 2000;
        private static readonly object _loginLock = new object();
        private static TrebuchetPrincipal principal;
        private readonly CsmClientConfiguration _csmConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsmClient"/> class.
        /// </summary>  
        /// <param name="config">
        /// The config.
        /// </param>
        public CsmClient(CsmClientConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _csmConfig = config;

            Login();
        }

        private DefaultBusinessObjectHandler<T> GetDefaultHandler<T>() where T : BusinessObjectModel, new()
        {
            return new DefaultBusinessObjectHandler<T>();
        }

        private BaseBusinessObjectHandler<T> GetHandler<T>() where T : BusinessObjectModel, new()
        {
            var handlerAttribute = typeof(T).GetCustomAttribute<HandledBusinessObjectAttribute>();
            if (handlerAttribute == null)
            {
                return GetDefaultHandler<T>();
            }

            if (!typeof(BaseBusinessObjectHandler<T>).IsAssignableFrom(handlerAttribute.Handler))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "You must use a type in the construction of your HandledBusinessObjectAttribute that is a BaseBusinessObjectHandler in the decoration of this class {0}",
                        typeof(T)));
            }

            return (BaseBusinessObjectHandler<T>)Activator.CreateInstance(handlerAttribute.Handler);
        }

        public void Create(ArbitraryCsmBusinessObject model)
        {
            var entityBo = TrebuchetApi.Api.BusObServices.CreateBusinessObjectByName(model.TypeName);
            SetBusObValues(entityBo, model);

            var result = entityBo.Save();

            if (result.Success)
            {
                model.RecId = entityBo.RecId;
            }
            else
            {
                throw new CherwellUpdateException("Business object creation failure " + result.ErrorText, result.ErrorText, result.ErrorText);
            }
        }

        public void Create<TBusinessObjectModel>(TBusinessObjectModel businessObjectModel) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            GetHandler<TBusinessObjectModel>().Create(businessObjectModel, new HandlerClient<TBusinessObjectModel>(this));
        }

        internal void CreateInCherwell<TBusinessObjectModel>(TBusinessObjectModel model) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            GetDefaultHandler<TBusinessObjectModel>().Create(model, new HandlerClient<TBusinessObjectModel>(this));
        }

        public void Delete<TBusinessObjectModel>(TBusinessObjectModel entity) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            GetHandler<TBusinessObjectModel>().Delete(entity, new HandlerClient<TBusinessObjectModel>(this));
        }

        internal void DeleteInCherwell<TBusinessObjectModel>(TBusinessObjectModel entity) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            GetDefaultHandler<TBusinessObjectModel>().Delete(entity, new HandlerClient<TBusinessObjectModel>(this));
        }

        public void DeleteByTableAndRecId(string tableName, string recId)
        {
            BusinessObjectDef busObDef;
            try
            {
                busObDef = TrebuchetApi.Api.DefinitionRepository.GetBusObDefByNameOrId(tableName);
            }
            catch (Exception e)
            {
                throw new CherwellDataException(string.Format("Errored getting {0}", tableName), e);
            }

            OperationResult result = null;
            try
            {
                result = TrebuchetApi.Api.BusObServices.DeleteBusObById(busObDef.Id, recId);
            }
            catch (TrebuchetSecurityException exception)
            {
                if (exception.Message == "Business object was not found.")
                {
                    // do nothing --its ok to delete something that doesn't exist. TODO - maybe return false and let the caller decide.
                    return;
                }

                throw;
            }

            if (result.Success)
            {
                return;
            }

            throw new CherwellUpdateException(result.ErrorText);
        }

        /// <summary>
        /// It is not performant to treat the client as a true disposable object, since the internal TREBUCHET connection is not disposed of - it either lingers and leaks memory
        /// Or we just keep the connection alive.
        /// </summary>
        public void Dispose()
        {
                // Do not logout... ever!
                // Logging in has such a terrible spin up time that we need to maintain the connection to Cherwell all the time.
                // Logout();
        }

        /// <summary>
        /// Retrieves the query from the clause and executes it
        /// </summary>
        /// <typeparam name="TBusinessObjectModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<TBusinessObjectModel> ExecuteQuery<TBusinessObjectModel>(ICsmQueryClause<TBusinessObjectModel> query)
            where TBusinessObjectModel : BusinessObjectModel, new()
        {
            return ExecuteQuery(query.EndClause());
        }

        public IEnumerable<ArbitraryCsmBusinessObject> ExecuteQuery(ICsmArbitraryQuery query)
        {
            var queryObject = (CsmArbitraryQuery)query;
            var trebuchetQuery = queryObject.GetTrebuchetQuery();

            if (trebuchetQuery == null)
            {
                throw new ArgumentNullException("query");
            }

            var result = TrebuchetApi.Api.BusObServices.ResolveQuery(trebuchetQuery);
            if (!result.Success)
            {
                throw new CherwellReadException(result.ErrorText);
            }

            var entities = new List<ArbitraryCsmBusinessObject>();
            foreach (DataRow dataRow in result.Data.Rows)
            {
                var entity = new ArbitraryCsmBusinessObject(queryObject.TypeName);
                SetEntityValues(entity, dataRow);
                entities.Add(entity);
            }

            return entities.AsEnumerable();
        }

        public IEnumerable<TBusinessObjectModel> ExecuteQuery<TBusinessObjectModel>(ICsmQuery<TBusinessObjectModel> query) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            return ExecuteQuery(query, false);
        }

        /// <summary>
        /// Returns a new ArbitraryQuery
        /// </summary>
        /// <returns>The Query</returns>
        public ICsmArbitraryQuery GetArbitraryQuery()
        {
            return new CsmArbitraryQuery();
        }

        public string GetBusinessObjectTypeId(string businessObjectTypeName)
        {
            var busObDef = TrebuchetApi.Api.DefinitionRepository.GetBusObDefByNameOrId(businessObjectTypeName);
            return busObDef.Id;
        }

        public ICsmQuery<TBusinessObjectModel> GetQuery<TBusinessObjectModel>() where TBusinessObjectModel : BusinessObjectModel, new()
        {
            return new CsmQuery<TBusinessObjectModel>();
        }

        /// <summary>
        /// Takes in a BusinessObject and a List of attachments to files,
        /// It then attaches those files to the object in cherwell
        /// </summary>
        /// <typeparam name="TBusinessObjectModel"></typeparam>
        /// <param name="entity"></param>
        /// <param name="attachments"></param>
        public void AttachFileToObject<TBusinessObjectModel>(TBusinessObjectModel entity, IEnumerable<Attachment> attachments)
            where TBusinessObjectModel : BusinessObjectModel
        {
            var businessObject = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(entity.TypeName, entity.RecId);
            foreach (var attachment in attachments)
            {
                var shortcut = attachment.GetShortcutInfoFromFileInfo();
                TrebuchetApi.Api.BusObServices.AttachmentService.UpdateDefinition(shortcut, null);
                businessObject.Shortcuts.Add(shortcut);
            }
            var status = businessObject.Save();
            if (!status.Success)
            {
                throw new CherwellDataException(string.Format("There were errors saving the businessObjectModel:{0} with the attachments:{1}", businessObject.PublicIdAutoPrefix, attachments.Select(attachment=>attachment.ShortCutText)));
            }
        }

        /// <summary>
        /// Takes in a BusinessObject and returns a list of MemoryStreams for 
        /// each attachment associated with it
        /// </summary>
        /// <typeparam name="TBusinessObjectModel"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEnumerable<Attachment> GetAttachmentsFromObject<TBusinessObjectModel>(TBusinessObjectModel entity)
            where TBusinessObjectModel : BusinessObjectModel
        {
            var output = new List<Attachment>();
            var businessObject = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(entity.TypeName, entity.RecId);
            foreach (ShortcutInfo shortcut in businessObject.Shortcuts)
            {
                Attachment attachment = null;
                var outMemoryStream = new MemoryStream();
                try
                {
                    TrebuchetApi.Api.BusObServices.AttachmentService.GetAttachment(shortcut.ShortcutTargetId, null, outMemoryStream);
                    attachment = new Attachment(shortcut, outMemoryStream);
                  
                }
                catch (TrebuchetWCFException ex)
                {
                    //...we didn't expect this,  gracefully handle it
                    attachment = new Attachment()
                    {
                        ShortCutText = shortcut.ShortcutText,
                        Error = ex.Message
                    };
                }
                
                output.Add(attachment);
                
            }
            return output;
        }

        public int HasAttachments<TBusinessObjectModel>(TBusinessObjectModel entity) where TBusinessObjectModel : BusinessObjectModel
        {
            var count = 0;
            var businessObject = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(entity.TypeName, entity.RecId);
            if (businessObject.Shortcuts != null)
            {
                count = businessObject.Shortcuts.Count;
            }
            return count;
        }

        public void LinkChildToParent<TChild, TParent>(Expression<Func<TParent, object>> relationshipPoiner, TChild related, string parentRecId) where TChild : BusinessObjectModel
            where TParent : BusinessObjectModel, new()
        {
            Link(related, relationshipPoiner, parentRecId);
        }

        public void LinkOneToOne<TChild, TParent>(Expression<Func<TParent, object>> relationshipPoiner, TChild related, string parentRecId) where TChild : BusinessObjectModel
            where TParent : BusinessObjectModel, new()
        {
            Link(related, relationshipPoiner, parentRecId, true);
        }

        public ArbitraryCsmBusinessObject ReadArbitraryModel(string objectName, string recId)
        {
            var cherwellEntity = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(objectName, recId);
            if (cherwellEntity != null)
            {
                var result = new ArbitraryCsmBusinessObject(objectName, recId);
                foreach (Field field in cherwellEntity.Fields)
                {
                    result.FieldsAndValues.Add(field.Def.Name, field.Value.ToString());
                }
                return result;
            }
            else
            {
                throw new CherwellReadException("The object requested was not found");
            }
        }

        public void UnlinkChildFromParent<TChild, TParent>(Expression<Func<TParent, object>> relationshipPointer, TChild child, string parentRecId)
            where TChild : BusinessObjectModel
            where TParent : BusinessObjectModel, new()
        {
            var childBo = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(child.TypeName, child.RecId);
            var relationship = GetRelationship(relationshipPointer, parentRecId);

            relationship.ClearLink(parentRecId, childBo.RecId);
        }

        public void ExecuteOneStep(string oneStepName, BusinessObjectModel businessObjectModel)
        {
            // One Step Definition
            var definitions = TrebuchetApi.Api.DefinitionRepository.GetAllDefinitionsOfType(OneStepDef.Class);
            var definitionStandIn = definitions.Cast<StandIn>().ToList().Where(x => x.Name.Equals(oneStepName, StringComparison.OrdinalIgnoreCase)).ToList().First();
            var definitionRequest = DefRequest.ByStandIn(definitionStandIn);
            var definition = TrebuchetApi.Api.DefinitionRepository.GetDefinition(definitionRequest) as OneStepDef;

            if (definition == null)
            {
                throw new OneStepNotFoundException(oneStepName, businessObjectModel.TypeName);
            }

            // Business Object Definition
            var businessObject = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(businessObjectModel.TypeName, businessObjectModel.RecId);

            // Record Provider: Query Cache Manager
            QueryDef query = QueryDef.CreateQuery();
            query.BusObId = businessObject.Def.Id;
            query.QueryResultType = QueryResultType.BusOb;
            var fieldId = businessObject.Def.Fields.GetFieldByName("recid").FullId;
            var clause = query.TopLevelGroupingClause.CreateFieldValueClause(fieldId, Operator.Equal, TypedValue.ForString(businessObjectModel.RecId));
            query.TopLevelGroupingClause.Clauses.Add(clause);
            QueryCacheManager queryManager = new QueryCacheManager(query);
            queryManager.Initialize(true);
            
            // Execute OneStep
            var result = TrebuchetApi.Api.ActionServices.RunOneStep(definition, businessObject, queryManager, OneStepRunFlags.RunCurrent);

            if (!result.Success)
            {
                throw new OneStepExecutionException(string.Format("An error ocurred while executing one step named {0} for business object named {1}. Failure Message from internals: {2}", oneStepName, businessObjectModel.TypeName, result.ErrorText));
            }

        }

        public void Update(ArbitraryCsmBusinessObject model)
        {
            var entityBo = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(model.TypeName, model.RecId);
            if (entityBo != null)
            {
                SetBusObValues(entityBo, model);

                var result = entityBo.Save();

                if (!result.Success)
                {
                    throw new CherwellUpdateException("Business object update failure " + result.ErrorText, result.ErrorText, result.ErrorText);
                }
            }
            else
            {
                throw new CherwellReadException("The object requested was not found");
            }
        }

        public void Update<TBusinessObjectModel>(TBusinessObjectModel businessObject) where TBusinessObjectModel : BusinessObjectModel, new()
        {
            GetHandler<TBusinessObjectModel>().Update(businessObject, new HandlerClient<TBusinessObjectModel>(this));
        }

        internal void UpdateInCherwell<TBusinessObjectModel>(TBusinessObjectModel businessObject)
            where TBusinessObjectModel : BusinessObjectModel, new()
        {
            GetDefaultHandler<TBusinessObjectModel>().Update(businessObject, new HandlerClient<TBusinessObjectModel>(this));
        }

        /// <summary>
        /// Links the TChild to the TParent via the relationship described by the expression (pointer)
        /// TChild and TParent are slightly misleading as it's not necessarily a child-parent relationship. TOne, TOther would also work...
        /// </summary>
        /// /// <typeparam name="TChild">
        /// The Many type of a One to Many relationship
        /// </typeparam>
        /// <typeparam name="TParent">
        /// The One type of a One to Many relationship
        /// </typeparam>
        /// <param name="child">
        /// The Many value of a One to Many relationship
        /// </param>
        /// <param name="pointer">
        /// An expression describing the foreign key of the relationship? 
        /// </param>
        /// <param name="parentId">
        /// The id of the "one" object
        /// </param>
        /// <param name="oneToOne">
        /// If set to TRUE, will overwrite any existing related value. If set to FALSE (default), will add TChild to TParent's relationship collection
        /// </param>
        protected virtual void Link<TChild, TParent>(TChild child, Expression<Func<TParent, object>> pointer, string parentId, bool oneToOne = false)
            where TChild : BusinessObjectModel where TParent : BusinessObjectModel, new()
        {
            var childBo = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(child.TypeName, child.RecId);
            var relationship = GetRelationship(pointer, parentId);

            // If the relationship between these two already exists, don't link them again
            if (relationship.SelectObjectByKey(childBo.RecId))
            {
                return;
            }

            // setting bChangeFirst to false ensures the record is added to the linked collection, otherwise LinkObject will just replace the existing record
            relationship.LinkObject(childBo, true, oneToOne);
        }

        /// <summary>
        /// The set entity values.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="dataRow">
        /// The data row.
        /// </param>
        protected virtual void SetEntityValues<T>(T entity, DataRow dataRow) where T : BusinessObjectModel, new()
        {
            var cherwellType = new T().TypeName;
            var busObDef = TrebuchetApi.Api.DefinitionRepository.GetBusObDefByNameOrId(cherwellType);
            var entityType = entity.GetType();
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi => Attribute.IsDefined(pi, typeof(FieldAttribute)));
            foreach (var propertyInfo in properties)
            {
                var field = propertyInfo.GetCustomAttribute<FieldAttribute>();

                var fieldByName = busObDef.Fields.GetFieldByNameThrowIfNotFound(field.Name, busObDef.Name);

                var typedValueFromCherwell = DataTableUtils.GetFieldValueFromDataRow(dataRow, busObDef, fieldByName.Id);

                dynamic value = GetTypedValue(propertyInfo, typedValueFromCherwell);

                propertyInfo.SetValue(entity, value);
            }
        }

        protected virtual void SetEntityValues(ArbitraryCsmBusinessObject entity, DataRow dataRow)
        {
            var cherwellType = entity.TypeName;
            var busObDef = TrebuchetApi.Api.DefinitionRepository.GetBusObDefByNameOrId(cherwellType);
            foreach (FieldDef fieldDef in busObDef.Fields.Values)
            {
                var typedValueFromCherwell = DataTableUtils.GetFieldValueFromDataRow(dataRow, busObDef, fieldDef.Id);

                string value = typedValueFromCherwell.ToString();

                entity.FieldsAndValues.Add(fieldDef.Name, value);
                if (fieldDef.Name.ToLower() == "recid")
                {
                    entity.RecId = value;
                }
            }
        }

        /// <summary>
        /// The set entity values.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="dataRow">
        /// The data row.
        /// </param>
        protected virtual void SetEntityValues(BusinessObjectModel entity, DataRow dataRow, Type realType)
        {
            var cherwellType = entity.TypeName;
            var busObDef = TrebuchetApi.Api.DefinitionRepository.GetBusObDefByNameOrId(cherwellType);

            if (busObDef == null)
            {
                throw new CherwellDataException(string.Format("Could not find business object definition corresponding to business object name {0}", cherwellType));
            }

            var entityType = realType;
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi => Attribute.IsDefined(pi, typeof(FieldAttribute)));
            foreach (var propertyInfo in properties)
            {
                var field = propertyInfo.GetCustomAttribute<FieldAttribute>();

                var fieldByName = busObDef.Fields.GetFieldByNameThrowIfNotFound(field.Name, busObDef.Name);

                var typedValueFromCherwell = DataTableUtils.GetFieldValueFromDataRow(dataRow, busObDef, fieldByName.Id);

                dynamic value = GetTypedValue(propertyInfo, typedValueFromCherwell);

                propertyInfo.SetValue(entity, value);
            }
        }

        private static void SetBusObValues(BusinessObject entityBo, ArbitraryCsmBusinessObject model)
        {
            foreach (var pair in model.FieldsAndValues)
            {
                var fieldDef = entityBo.Def.Fields.GetFieldByName(pair.Key);
                var field = entityBo.GetField(fieldDef.Id);
                var updateTransaction = field.SetValue(TypedValue.ForString(pair.Value));
                if (!updateTransaction.Success)
                {
                    throw new CherwellUpdateException(string.Format("Business object update failure for {0}: {1}", fieldDef.Name, updateTransaction.ErrorText), updateTransaction.ErrorText, updateTransaction.ErrorText);
                }
            }
        }

        private IEnumerable<TBusinessObjectModel> ExecuteQuery<TBusinessObjectModel>(ICsmQuery<TBusinessObjectModel> query, bool isSubquery)
            where TBusinessObjectModel : BusinessObjectModel, new()
        {
            // We accept the interface, but cast it to our implementation here to access the internal methods
            // The interface is provided for unit testing in client code
            var csmQuery = (CsmQuery<TBusinessObjectModel>)query;
            var trebuchetQuery = csmQuery.GetTrebuchetQuery();

            if (trebuchetQuery == null)
            {
                throw new ArgumentNullException("query");
            }

            var data = GetQueryResultData(trebuchetQuery, csmQuery.IsSingleRecordQuery);

            var entities = new List<TBusinessObjectModel>();
            foreach (DataRow dataRow in data.Rows)
            {
                var entity = new TBusinessObjectModel();
                SetEntityValues(entity, dataRow);
                entities.Add(entity);
            }

            // Recursively resolve sub-queries
            if (!isSubquery)
            {
                foreach (var businessObjectModel in entities)
                {
                    foreach (var subQueryDef in csmQuery.SubQueries)
                    {
                        // Figure out which property of the parent object is being resolved by this query
                        var prop = businessObjectModel.GetType().GetProperty(subQueryDef.DestinationPropertyName);

                        // Get a trebuchet query for this sub-query
                        var internalQuery = csmQuery.ForSubQuery(businessObjectModel.TypeName, businessObjectModel.RecId, subQueryDef.RelationshipName);
                        var subResults = ExecuteSubquery(subQueryDef.Type, internalQuery);

                        // We always return a collection from ExecuteQuery, even if it's just a single object (simpler API)
                        // But if our destination property is not a collection, need to get the type inside the collection before we set its value
                        if (subQueryDef.IsCollection)
                        {
                            subQueryDef.SetValue(prop, businessObjectModel, subResults);
                        }
                        else
                        {
                            var singleResult = subResults.FirstOrDefault();
                            subQueryDef.SetValue(prop, businessObjectModel, singleResult);
                        }
                    }
                }
            }

            return entities.AsEnumerable();
        }

        private DataTable GetQueryResultData(QueryDef trebuchetQuery, bool isSingleRecordQuery)
        {
            QueryResult result;

            // The QueryCacheManager has overhead that we don't need for a single record query - but it's useful for larger data sets
            if (isSingleRecordQuery)
            {
                result = TrebuchetApi.Api.BusObServices.ResolveQuery(trebuchetQuery);
                if (result.Success)
                {
                    return result.Data;
                }

                throw new CherwellReadException(result.ErrorText);
            }

            var queryManager = CreateQueryManager(trebuchetQuery);
            result = queryManager.GetFirstPage();
            if (result == null)
            {
                // The query manager returns null in the case of an empty results set
                return new DataTable();
            }
            
            if (!result.Success)
            {
                throw new CherwellReadException(result.ErrorText);
            }

            var data = result.Data;
            while (result.Count == queryManager.PageSize)
            {
                result = queryManager.GetNextPage();
                if (!result.Success)
                {
                    throw new CherwellReadException(result.ErrorText);
                }

                data.Merge(result.Data);
            }

            return data;
        }

        private QueryCacheManager CreateQueryManager(QueryDef trebuchetQuery)
        {
            var queryManager = new QueryCacheManager(trebuchetQuery);
            queryManager.Initialize(true);
            if (queryManager.HasError)
            {
                throw new CherwellReadException(queryManager.LastError);
            }

            // Per Cherwell's Mark Clayton, setting this flag causes the SQL generator within the Trebuchet API to create a query that's optimized for retrieving large data sets
            // Without this, the SQL that's generated is very inefficient.
            queryManager.SetGridMode();

            // CsmMagic does not currently support pagination or data sets larger than 2,000 - but there's no reason it couldn't in the future
            queryManager.PageSize = _maxPageSize;

            // Setting this increases the max records that can be retrieved from 2,000 to 100,000
            queryManager.SetNeedAllKeys(true);

            return queryManager;
        }

        private IEnumerable<BusinessObjectModel> ExecuteSubquery(Type objectType, QueryDef query)
        {
            var result = GetQueryResultData(query, false);

            var entities = new List<BusinessObjectModel>();
            foreach (DataRow dataRow in result.Rows)
            {
                var entity = (BusinessObjectModel)Activator.CreateInstance(objectType);
                SetEntityValues(entity, dataRow, objectType);
                entities.Add(entity);
            }

            return entities;
        }

        private Relationship GetRelationship<TParent>(Expression<Func<TParent, object>> relationshipPointer, string parentRecId)
            where TParent : BusinessObjectModel, new()
        {
            var parentType = new TParent().TypeName;
            var parentBo = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(parentType, parentRecId);
            var relationshipName = FieldHelper.GetCsmRelationshipNameFromMember(relationshipPointer);
            var relationship = parentBo.GetRelationshipByName(relationshipName);
            return relationship;
        }

        private dynamic GetTypedValue(PropertyInfo propertyInfo, TypedValue typedValueFromCherwell)
        {
            var t = propertyInfo.PropertyType;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                t = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
            }

            if (t == typeof(bool))
            {
                return typedValueFromCherwell.ToBool();
            }

            if (t == typeof(string))
            {
                return typedValueFromCherwell.ToString();
            }

            if (t == typeof(DateTime))
            {
                return typedValueFromCherwell.ToDateTime();
            }

            if (t == typeof(decimal))
            {
                return typedValueFromCherwell.ToNumber();
            }

            if (t == typeof(int))
            {
                return (int)typedValueFromCherwell.ToNumber();
            }

            if (t == typeof(long))
            {
                return (long)typedValueFromCherwell.ToNumber();
            }

            if (t == typeof(float))
            {
                return (float)typedValueFromCherwell.ToNumber();
            }

            if (t == typeof(double))
            {
                return (double)typedValueFromCherwell.ToNumber();
            }

            throw new InvalidOperationException("Cannot set entity value for this type of value: " + propertyInfo.PropertyType.FullName);
        }

        /// <summary>
        ///     Logs in the user in the configuration settings.
        /// </summary>
        private void Login()
        {
            if (!TrebuchetApi.Api.Connected)
            {
                TrebuchetApi.Api.Connect(_csmConfig.ConnectionName);
            }

            if (principal != null && principal.Identity.IsAuthenticated)
            {
                // This shares the principal between http request threads.
                // Trebuchet checks the principal to see if the user is logged in.
                // We are retaining that principal in a static variable and applying
                // it to the current thread in order to maintain a logged-in state.
                Thread.CurrentPrincipal = principal;
            }

            lock (_loginLock)
            {
                if (TrebuchetApi.Api.LoggedIn)
                {
                    return;
                }

                // Login using explicit credentials
                if (TrebuchetApi.Api.Login(
                    ApplicationType.RichClient, 
                    _csmConfig.Username, 
                    _csmConfig.Password, 
                    false, 
                    LicensedProductCode.CherwellServiceDesk, 
                    "SAM", 
                    "Sample Application"))
                {
                    principal = (TrebuchetPrincipal)Thread.CurrentPrincipal;
                    return;
                }
            }

            if (TrebuchetApi.Api.Connected)
            {
                TrebuchetApi.Api.Disconnect();
            }
        }
    }
}
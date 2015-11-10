using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using CsmMagic.Models;
using CsmMagic.Queries;

namespace CsmMagic
{
    /// <summary>
    ///     This interface is the point-of-entry for the CsmMagic library. It interacts directly with Cherwell to perform CRUD operations on business objects.
    /// </summary>
    public interface ICsmClient : IDisposable
    {
        /// <summary>
        /// Creates the provided business object in the Cherwell database.
        /// </summary>
        /// <typeparam name="TBusinessObjectModel"></typeparam>
        /// <param name="businessObjectModel">Sets the new RecId on the provided object</param>
        void Create<TBusinessObjectModel>(TBusinessObjectModel businessObjectModel) where TBusinessObjectModel : BusinessObjectModel, new();

        void Create(ArbitraryCsmBusinessObject model);

        /// <summary>
        /// Tries to delete the specified business object
        /// </summary>
        /// <typeparam name="TBusinessObjectModel"></typeparam>
        /// <param name="recId"></param>
        void Delete<TBusinessObjectModel>(TBusinessObjectModel entity) where TBusinessObjectModel : BusinessObjectModel, new();

        /// <summary>
        /// Executes the provided query and returns a collection of business objects
        /// </summary>
        /// <typeparam name="TBusinessObjectModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<TBusinessObjectModel> ExecuteQuery<TBusinessObjectModel>(ICsmQuery<TBusinessObjectModel> query) where TBusinessObjectModel : BusinessObjectModel, new();

        /// <summary>
        /// Finalizes the query clause, then executes the resulting query
        /// </summary>
        /// <typeparam name="TBusinessObjectModel"></typeparam>
        /// <param name="clause"></param>
        /// <returns></returns>
        IEnumerable<TBusinessObjectModel> ExecuteQuery<TBusinessObjectModel>(ICsmQueryClause<TBusinessObjectModel> clause) where TBusinessObjectModel : BusinessObjectModel, new();

        /// <summary>
        /// Returns a new ArbitraryQuery
        /// </summary>
        /// <returns>The Query</returns>
        ICsmArbitraryQuery GetArbitraryQuery();

        /// <summary>
        /// Returns the RecId of the specified business object definition
        /// </summary>
        /// <param name="businessObjectTypeName"></param>
        /// <returns></returns>
        string GetBusinessObjectTypeId(string businessObjectTypeName);

        /// <summary>
        /// Returns a new query
        /// </summary>
        /// <typeparam name="TBusinessObjectModel"></typeparam>
        /// <returns></returns>
        ICsmQuery<TBusinessObjectModel> GetQuery<TBusinessObjectModel>() where TBusinessObjectModel : BusinessObjectModel, new();

        /// <summary>
        /// Links the TChild to the TParent via the relationship described by the expression (pointer) as part of a many-to-one relationship
        /// TChild and TParent are slightly misleading as it's not necessarily a child-parent relationship. TOne, TOther would also work...
        /// </summary>
        /// /// <typeparam name="TChild">
        /// The Many type of a One to Many relationship
        /// </typeparam>
        /// <typeparam name="TParent">
        /// The One type of a One to Many relationship
        /// </typeparam>
        /// <param name="relationshipPointer"></param>
        /// <param name="related"></param>
        /// <param name="parentRecId"></param>
        void LinkChildToParent<TChild, TParent>(Expression<Func<TParent, object>> relationshipPointer, TChild related, string parentRecId) where TChild : BusinessObjectModel
            where TParent : BusinessObjectModel, new();

        /// <summary>
        /// Links two objects together in a one-to-one relationship
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <typeparam name="TParent"></typeparam>
        /// <param name="relationshipPointer"></param>
        /// <param name="related"></param>
        /// <param name="parentRecId"></param>
        void LinkOneToOne<TChild, TParent>(Expression<Func<TParent, object>> relationshipPointer, TChild related, string parentRecId) where TChild : BusinessObjectModel
            where TParent : BusinessObjectModel, new();

        void UnlinkChildFromParent<TChild, TParent>(
            Expression<Func<TParent, object>> relationshipPointer,
            TChild child,
            string parentRecId) where TChild : BusinessObjectModel where TParent : BusinessObjectModel, new();

        ArbitraryCsmBusinessObject ReadArbitraryModel(string objectName, string recId);

        /// <summary>
        /// Updates the specified business object in Cherwell to have the values of the provided object
        /// </summary>
        /// <typeparam name="TBusinessObjectModel"></typeparam>
        /// <param name="businessObject"></param>
        void Update<TBusinessObjectModel>(TBusinessObjectModel businessObject) where TBusinessObjectModel : BusinessObjectModel, new();

        void Update(ArbitraryCsmBusinessObject model);

        IEnumerable<ArbitraryCsmBusinessObject> ExecuteQuery(ICsmArbitraryQuery query);

        void DeleteByTableAndRecId(string tableName, string id);

        void AttachFileToObject<TBusinessObjectModel>(TBusinessObjectModel entity, IEnumerable<Attachment> attachments)
            where TBusinessObjectModel : BusinessObjectModel;

        IEnumerable<Attachment> GetAttachmentsFromObject<TBusinessObjectModel>(TBusinessObjectModel entity)
            where TBusinessObjectModel : BusinessObjectModel;

        int HasAttachments<TBusinessObjectModel>(TBusinessObjectModel entity)
            where TBusinessObjectModel : BusinessObjectModel;
        void ExecuteOneStep(string oneStepName, BusinessObjectModel businessObjectModel);

    }
}
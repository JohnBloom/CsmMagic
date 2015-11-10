using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CsmMagic.Attributes;
using CsmMagic.Exceptions;
using CsmMagic.Models;
using Trebuchet;
using Trebuchet.API;
using Expression = System.Linq.Expressions.Expression;

namespace CsmMagic.Queries
{
    /// <summary>
    /// A fluent builder interface for creating a trebuchet query.
    /// Continue to extend the interface by adding more methods as required
    /// </summary>
    /// <typeparam name="T">A BusinessObjectModel with properties decorated with [Field] or [Relationship]</typeparam>
    public class CsmQuery<T> : ICsmQuery<T> where T : BusinessObjectModel, new()
    {
        /// <summary>
        /// This constructor creates a BusOb query for the type of T
        /// </summary>
        public CsmQuery()
        {
            var thisType = new T();
            ForType(thisType.TypeName);

            SubQueries = new List<CsmSubQuery>();
        }

        protected CsmQuery(QueryDef query) : this()
        {
            TrebuchetQuery = query;
        }

        internal List<CsmSubQuery> SubQueries { get; set; }

        internal QueryDef TrebuchetQuery { get; set; }

        internal bool IsSingleRecordQuery { get; set; }

        internal BusinessObjectDef TypeDefinition { get; set; }

        protected string TypeName { get; set; }

        /// <summary>
        /// Overwrites the query's business object definition with the provided definition
        /// </summary>
        /// <param name="businessObjectDefinitionName"></param>
        /// <returns></returns>
        public ICsmQuery<T> ForBusinessObject(string businessObjectDefinitionName)
        {
            TypeDefinition = TrebuchetApi.Api.DefinitionRepository.GetBusObDefByNameOrId(businessObjectDefinitionName);
            TrebuchetQuery.BusObId = TypeDefinition.Id;
            return this;
        }

        public ICsmQuery<TRelated> ForChildren<TRelated>(Expression<Func<T, IEnumerable<TRelated>>> childrenExpression, string parentId) where TRelated : BusinessObjectModel, new()
        {
            var relationshipName = FieldHelper.GetCsmRelationshipNameFromMember(childrenExpression);
            return ByRelationship<TRelated>(TypeName, parentId, relationshipName);
        }

        public ICsmQuery<T> ForKey(string key)
        {
            ForSingleRecord();
            
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi => Attribute.IsDefined(pi, typeof(KeyAttribute)));
            foreach (var property in properties)
            {
                var field = property.GetCustomAttribute<FieldAttribute>();

                if (field == null)
                {
                    throw new InvalidDataException("A key attribute cannot be used without a field attribute");    
                }

                TrebuchetQuery = TrebuchetQuery.OrWhere(TypeDefinition, field.Name, CsmQueryOperator.Equal, key);    
            }
            
            return this;
        }

        public ICsmQuery<T> ForRecId(string recId)
        {
            ForSingleRecord();
            TrebuchetQuery = TrebuchetQuery.AndWhere(TypeDefinition, "RecID", CsmQueryOperator.Equal, recId);
            return this;
        }

        /// <summary>
        /// Optimizes the query to select a single record
        /// </summary>
        /// <returns></returns>
        public ICsmQuery<T> ForSingleRecord()
        {
            TrebuchetQuery.TopCount = 1;
            IsSingleRecordQuery = true;
            return this;
        }        

        /// <summary>
        /// Includes children in the request to get the object. There can be some performance issues when calling this when retreiving a list.
        /// </summary>
        /// <param name="childrenExpression"></param>
        /// <returns></returns>
        /// <remarks>This overload is used for collection properties. This ensures that lower-level code doesn't try to retrieve IEnumerable[TRelated] for example instead of [TRelated]</remarks>
        public ICsmQuery<T> Include<TRelated>(Expression<Func<T, IEnumerable<TRelated>>> childrenExpression) where TRelated : class
        {
            return Include<TRelated>((MemberExpression)childrenExpression.Body, childrenExpression.Parameters.Single(), true);
        }

        /// <summary>
        /// Includes children in the request to get the object. There can be some performance issues when calling this when retreiving a list.
        /// </summary>
        /// <param name="childExpression"></param>
        /// <returns></returns>
        /// <remarks>This overload is used for single member properties</remarks>
        public ICsmQuery<T> Include<TRelated>(Expression<Func<T, TRelated>> childExpression) where TRelated : class
        {
            return Include<TRelated>((MemberExpression)childExpression.Body, childExpression.Parameters.Single(), false);
        }

        /// <summary>
        /// Creates a generic where statement
        /// </summary>
        /// <param name="fieldExpression"></param>
        /// <param name="op"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        public ICsmQueryClause<T> Where(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, string filterValue)
        {
            var queryClause = new CsmQueryClause<T>(this).And(fieldExpression, op, filterValue);
            return queryClause;
        }

        /// <summary>
        /// Creates a generic where statement
        /// </summary>
        /// <param name="fieldExpression"></param>
        /// <param name="op"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        public ICsmQueryClause<T> Where(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, bool filterValue)
        {
            var queryClause = new CsmQueryClause<T>(this).And(fieldExpression, op, filterValue);
            return queryClause;
        }

        /// <summary>
        /// Creates a generic where statement
        /// </summary>
        /// <param name="fieldExpression"></param>
        /// <param name="op"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        public ICsmQueryClause<T> Where(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, DateTime filterValue)
        {
            var queryClause = new CsmQueryClause<T>(this).And(fieldExpression, op, filterValue);
            return queryClause;
        }

        /// <summary>
        /// Creates a generic where statement
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public ICsmQueryClause<T> Where(Expression<Func<T, bool>> predicate)
        {
            ICsmQueryClause<T> queryClause = new CsmQueryClause<T>(this);
            queryClause = predicate.NodeType == ExpressionType.Lambda 
                ? predicate.Body.NodeType == ExpressionType.OrElse 
                ? queryClause.Or(predicate) : queryClause.And(predicate) 
                : queryClause.And(predicate);

            return queryClause;
        }

        public ICsmQueryClause<T> WhereRelated<TRelated>(
            Expression<Func<T, TRelated>> relationshipExpression, 
            Expression<Func<TRelated, object>> fromFieldExpression, 
            CsmQueryOperator op, 
            string value) where TRelated : BusinessObjectModel, new()
        {
            var queryClause = new CsmQueryClause<T>(this).AndRelated(relationshipExpression, fromFieldExpression, op, value);
            return queryClause;
        }

        internal CsmQuery<TRelated> ByRelationship<TRelated>(string parentTypeDefName, string relatedValueRecId, string relationshipName)
            where TRelated : BusinessObjectModel, new()
        {
            ForSubQuery(parentTypeDefName, relatedValueRecId, relationshipName);

            return new CsmQuery<TRelated>(TrebuchetQuery);
        }

        internal QueryDef ForSubQuery(string parentDefName, string relatedValueRecId, string relationshipName)
        {
            var csmBo = TrebuchetApi.Api.BusObServices.GetBusinessObjectByNameAndRecId(parentDefName, relatedValueRecId);
            if (csmBo == null)
            {
                throw new CherwellDataException(string.Format("Could not find data of type {0} for ID {1}", parentDefName, relatedValueRecId));
            }

            var relationship = csmBo.GetRelationshipByName(relationshipName);
            var query = relationship.GetQueryForRelatedData();
            TrebuchetQuery = query;
            return query;
        }

        /// <summary>
        /// Returns the query (finishes the build)
        /// </summary>
        /// <returns></returns>
        internal QueryDef GetTrebuchetQuery()
        {
            return TrebuchetQuery;
        }

        protected CsmQuery<T> ForType(string typeDefName)
        {
            TrebuchetQuery = QueryDef.CreateQuery();
            TypeDefinition = TrebuchetApi.Api.DefinitionRepository.GetBusObDefByNameOrId(typeDefName);
            if (TypeDefinition == null)
            {
                throw new CherwellDataException(string.Format("Could not find business object definition corresponding to business object name {0}", typeDefName));
            }

            TrebuchetQuery.BusObId = TypeDefinition.Id;
            TrebuchetQuery.QueryResultType = QueryResultType.FieldList;
            SetFieldsOnQuery(TrebuchetQuery);
            TypeName = typeDefName;
            return this;
        }

        private void SetFieldsOnQuery(QueryDef trebuchetQuery)
        {
            var fields = typeof(T).GetMembers().Where(m => Attribute.IsDefined(m, typeof(FieldAttribute))).Select(m => m.GetCustomAttribute<FieldAttribute>()).ToList();
            fields.ForEach(f =>
            {
                var fieldDef = TypeDefinition.Fields.GetFieldByNameThrowIfNotFound(f.Name, TypeDefinition.Name);
                trebuchetQuery.Fields.Add(fieldDef.Id);
            });
        }
        
        private ICsmQuery<T> Include<TRelated>(MemberExpression destinationProperty, ParameterExpression source, bool isCollection) where TRelated : class
        {
            var destinationMember = destinationProperty.Member;
            var newExpression = Expression.MakeMemberAccess(source, destinationMember);
            var relationshipName = FieldHelper.GetCsmRelationshipNameFromMember(newExpression);

            var csq = new CsmSubQuery<TRelated>
                          {
                              Type = typeof(TRelated), 
                              DestinationPropertyName = destinationMember.Name, 
                              RelationshipName = relationshipName, 
                              IsCollection = isCollection
                          };
            SubQueries.Add(csq);
            return this;
        }
    }
}
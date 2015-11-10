using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CsmMagic.Models;

namespace CsmMagic.Queries
{
    /// <summary>
    /// A fluent builder interface for creating a trebuchet query.
    /// Continue to extend the interface by adding more methods as required
    /// </summary>
    /// <typeparam name="T">A BusinessObjectModel with properties decorated with [Field] or [Relationship]</typeparam>
    public interface ICsmQuery<T> where T : BusinessObjectModel, new()
    {
        /// <summary>
        /// Overwrites the query's business object definition with the provided definition
        /// </summary>
        /// <param name="businessObjectDefinitionName"></param>
        /// <returns></returns>
        ICsmQuery<T> ForBusinessObject(string businessObjectDefinitionName);

        /// <summary>
        /// Converts this query from a query of T to a query of TChild by examining the relationship pointed to by the childrenExpression
        /// Retrieves TChild which belong to the parentId
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="childrenExpression"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        ICsmQuery<TChild> ForChildren<TChild>(Expression<Func<T, IEnumerable<TChild>>> childrenExpression, string parentId) where TChild : BusinessObjectModel, new();

        /// <summary>
        /// Returns the T specified by the key. Keys are properties that are maked with the [Key] and [Field] attributes.
        /// </summary>
        /// <param name="recId"></param>
        /// <returns></returns>
        ICsmQuery<T> ForKey(string recId);

        /// <summary>
        /// Returns the T specified by the rec ID
        /// </summary>
        /// <param name="recId"></param>
        /// <returns></returns>
        ICsmQuery<T> ForRecId(string recId);

        /// <summary>
        /// Optimizes the query to select a single record
        /// </summary>
        /// <returns></returns>
        ICsmQuery<T> ForSingleRecord();


        /// <summary>
        /// Begins a query clause with the specified filter using just the expression
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="op"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        ICsmQueryClause<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Begins a query clause with the specified filter
        /// </summary>
        /// <param name="fieldExpression"></param>
        /// <param name="op"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        ICsmQueryClause<T> Where(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, string filterValue);

        /// <summary>
        /// Begins a query clause with the specified filter
        /// </summary>
        /// <param name="fieldExpression"></param>
        /// <param name="op"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        ICsmQueryClause<T> Where(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, bool filterValue);

        /// <summary>
        /// Begins a query clause with the specified filter
        /// </summary>
        /// <param name="fieldExpression"></param>
        /// <param name="op"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        ICsmQueryClause<T> Where(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, DateTime filterValue);

        /// <summary>
        /// Includes children in the request to get the object. There can be some performance issues when calling this when retreiving a list.
        /// </summary>
        /// <param name="childrenExpression"></param>
        /// <returns></returns>
        ICsmQuery<T> Include<TRelated>(Expression<Func<T, IEnumerable<TRelated>>> childrenExpression) where TRelated : class;

        /// <summary>
        /// Includes children in the request to get the object. There can be some performance issues when calling this when retreiving a list.
        /// </summary>
        /// <param name="childExpression"></param>
        /// <returns></returns>
        ICsmQuery<T> Include<TRelated>(Expression<Func<T, TRelated>> childExpression) where TRelated : class;

        ICsmQueryClause<T> WhereRelated<TRelated>(
            Expression<Func<T, TRelated>> relationshipExpression,
            Expression<Func<TRelated, object>> fromFieldExpression,
            CsmQueryOperator op,
            string value) where TRelated : BusinessObjectModel, new();
    }
}
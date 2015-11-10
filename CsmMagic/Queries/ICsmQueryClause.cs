using System;
using System.Linq.Expressions;
using CsmMagic.Models;

namespace CsmMagic.Queries
{
    public interface ICsmQueryClause<T> where T : BusinessObjectModel, new()
    {
        ICsmQueryClause<T> And(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, string filterValue);

        ICsmQueryClause<T> Or(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, string filterValue);

        ICsmQueryClause<T> And(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, bool filterValue);

        ICsmQueryClause<T> Or(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, bool filterValue);

        ICsmQueryClause<T> And(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, DateTime filterValue);

        ICsmQueryClause<T> Or(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, DateTime filterValue);

        ICsmQueryClause<T> And(Expression<Func<T, bool>> predicate);

        ICsmQueryClause<T> Or(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Finishes the chain of clauses and returns the query
        /// </summary>
        /// <returns></returns>
        ICsmQuery<T> EndClause();
    }
}
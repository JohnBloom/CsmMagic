using System;
using System.Linq.Expressions;
using CsmMagic.Models;
using Trebuchet;
using Trebuchet.API;

namespace CsmMagic.Queries
{
    public class CsmQueryClause<T> : ICsmQueryClause<T> where T : BusinessObjectModel, new()
    {
        private readonly CsmQuery<T> _storedQuery;

        // Delightfully, the trebuchet query API only allows to use AND clauses or OR clauses, you can't mix them
        // If you have some AND clauses, and you add an OR clause, it retroactively sets all your previous AND clauses to be OR clauses instead.
        // And vice-versa.
        protected bool IsUsingAndQueryClauses;
        protected bool IsUsingOrQueryClauses;

        public CsmQueryClause(CsmQuery<T> csmQuery)
        {
            _storedQuery = csmQuery;
        }

        public ICsmQueryClause<T> And(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, string filterValue)
        {
            var fieldName = FieldHelper.GetCsmFieldNameFromMember(fieldExpression);
            return And(fieldName, op, filterValue);
        }

        public ICsmQueryClause<T> And(Expression<Func<T, bool>> predicate)
        {
            ValidateAndQuerySemanticsConsistency();
            var queries = CsmVisitor.GetQueries(predicate);
            foreach (var query in queries)
            {
                if (query.IsOr)
                {
                    throw new InvalidOperationException("Cannot combine AND and OR clauses in a CsmQuery - you must use two separate queries.");
                }

                _storedQuery.TrebuchetQuery = _storedQuery.TrebuchetQuery.AndWhere(_storedQuery.TypeDefinition, query.Property, query.Operator, query.Value);
            }

            return this;
        }

        public ICsmQueryClause<T> Or(Expression<Func<T, bool>> predicate)
        {
            ValidateOrQuerySemanticsConsistency();
            var queries = CsmVisitor.GetQueries(predicate);
            foreach (var query in queries)
            {
                if (!query.IsOr)
                {
                    throw new InvalidOperationException("Cannot combine AND and OR clauses in a CsmQuery - you must use two separate queries.");
                }

                _storedQuery.TrebuchetQuery = _storedQuery.TrebuchetQuery.OrWhere(_storedQuery.TypeDefinition, query.Property, query.Operator, query.Value);
            }

            return this;
        }

        public ICsmQueryClause<T> Or(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, string filterValue)
        {
            var fieldName = FieldHelper.GetCsmFieldNameFromMember(fieldExpression);
            return Or(fieldName, op, filterValue);
        }

        public ICsmQueryClause<T> And(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, bool filterValue)
        {
            var fieldName = FieldHelper.GetCsmFieldNameFromMember(fieldExpression);
            return And(fieldName, op, filterValue);
        }

        public ICsmQueryClause<T> Or(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, bool filterValue)
        {
            var fieldName = FieldHelper.GetCsmFieldNameFromMember(fieldExpression);
            return Or(fieldName, op, filterValue);
        }

        public ICsmQueryClause<T> And(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, DateTime filterValue)
        {
            var fieldName = FieldHelper.GetCsmFieldNameFromMember(fieldExpression);
            return And(fieldName, op, filterValue);
        }

        public ICsmQueryClause<T> Or(Expression<Func<T, object>> fieldExpression, CsmQueryOperator op, DateTime filterValue)
        {
            var fieldName = FieldHelper.GetCsmFieldNameFromMember(fieldExpression);
            return Or(fieldName, op, filterValue);
        }

        public ICsmQuery<T> EndClause()
        {
            return _storedQuery;
        }

        protected ICsmQueryClause<T> And(string fieldName, CsmQueryOperator op, string value)
        {
            ValidateAndQuerySemanticsConsistency();
            _storedQuery.TrebuchetQuery = _storedQuery.TrebuchetQuery.AndWhere(_storedQuery.TypeDefinition, fieldName, op, value);
            return this;
        }

        protected ICsmQueryClause<T> Or(string fieldName, CsmQueryOperator op, string value)
        {
            ValidateOrQuerySemanticsConsistency();
            _storedQuery.TrebuchetQuery = _storedQuery.TrebuchetQuery.OrWhere(_storedQuery.TypeDefinition, fieldName, op, value);
            return this;
        }

        protected ICsmQueryClause<T> And(string fieldName, CsmQueryOperator op, bool value)
        {
            ValidateAndQuerySemanticsConsistency();
            _storedQuery.TrebuchetQuery = _storedQuery.TrebuchetQuery.AndWhere(_storedQuery.TypeDefinition, fieldName, op, value);
            return this;
        }

        protected ICsmQueryClause<T> Or(string fieldName, CsmQueryOperator op, bool value)
        {
            ValidateOrQuerySemanticsConsistency();
            _storedQuery.TrebuchetQuery = _storedQuery.TrebuchetQuery.OrWhere(_storedQuery.TypeDefinition, fieldName, op, value);
            return this;
        }

        protected ICsmQueryClause<T> And(string fieldName, CsmQueryOperator op, DateTime value)
        {
            ValidateAndQuerySemanticsConsistency();
            _storedQuery.TrebuchetQuery = _storedQuery.TrebuchetQuery.AndWhere(_storedQuery.TypeDefinition, fieldName, op, value);
            return this;
        }

        public ICsmQueryClause<T> AndRelated<TRelated>(Expression<Func<T, TRelated>> relationshipExpression, Expression<Func<TRelated, object>> fromFieldExpression, CsmQueryOperator op, string value) where TRelated : BusinessObjectModel, new()
        {
            var related = new TRelated();
            ValidateAndQuerySemanticsConsistency();
            BusinessObjectDef bodCustomerContact = TrebuchetApi.Api.DefinitionRepository.GetDefinition(DefRequest.ByName(BusinessObjectDef.Class, related.TypeName)) as BusinessObjectDef;

            var relationshipName = FieldHelper.GetCsmRelationshipNameFromMember(relationshipExpression);
            var fieldName = FieldHelper.GetCsmFieldNameFromMember(fromFieldExpression);

            _storedQuery.TrebuchetQuery = _storedQuery.TrebuchetQuery.AndWhereRelated(_storedQuery.TypeDefinition, bodCustomerContact, relationshipName, fieldName, op, value);
            return this;
        }

        protected ICsmQueryClause<T> Or(string fieldName, CsmQueryOperator op, DateTime value)
        {
            ValidateOrQuerySemanticsConsistency();
            _storedQuery.TrebuchetQuery = _storedQuery.TrebuchetQuery.OrWhere(_storedQuery.TypeDefinition, fieldName, op, value);
            return this;
        }

        /// <summary>
        /// Ensures that you cannot accidentally combine AND and OR queries in a single query (this would lead to unpredictable/unexpected behavior from the query, as Cherwell doesn't support mixing the two predicates)
        /// TODO: Confirm that this is actually true, there's some uncertainty around how Cherwell behaves in the mixed case
        /// </summary>
        private void ValidateAndQuerySemanticsConsistency()
        {
            if (!IsUsingAndQueryClauses && !IsUsingOrQueryClauses)
            {
                IsUsingAndQueryClauses = true;
            }

            if (!IsUsingAndQueryClauses || IsUsingOrQueryClauses)
            {
                throw new InvalidOperationException("Cannot combine AND and OR clauses in a CsmQuery - you must use two separate queries.");
            }
        }

        /// <summary>
        /// Ensures that you cannot accidentally combine AND and OR queries in a single query (this would lead to unpredictable/unexpected behavior from the query, as Cherwell doesn't support mixing the two predicates)
        /// TODO: Confirm that this is actually true, there's some uncertainty around how Cherwell behaves in the mixed case
        /// </summary>
        private void ValidateOrQuerySemanticsConsistency()
        {
            if (!IsUsingOrQueryClauses && !IsUsingAndQueryClauses)
            {
                IsUsingOrQueryClauses = true;
            }

            if (!IsUsingOrQueryClauses || IsUsingAndQueryClauses)
            {
                throw new InvalidOperationException("Cannot combine AND and OR clauses in a CsmQuery - you must use two separate queries.");
            }
        }
    }
}
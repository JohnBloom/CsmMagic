using System;
using System.Collections.Generic;
using CsmMagic.Models;
using Trebuchet;
using Trebuchet.API;

namespace CsmMagic
{
    internal static class TrebuchetQueryExtensions
    {
        /// <summary>
        /// Avoids exposing the Trebuchet enumeration to client code
        /// </summary>
        internal static Dictionary<CsmQueryOperator, Operator> OperatorMap = new Dictionary<CsmQueryOperator, Operator>
                                                                                 {
                                                                                     { CsmQueryOperator.Equal, Operator.Equal }, 
                                                                                     { CsmQueryOperator.NotEqual, Operator.NotEqual }, 
                                                                                     { CsmQueryOperator.GreaterThan, Operator.GreaterThan }, 
                                                                                     { CsmQueryOperator.LessThan, Operator.LessThan },
                                                                                     { CsmQueryOperator.NotEmpty, Operator.NotEmpty},
                                                                                     { CsmQueryOperator.Empty, Operator.Empty },
                                                                                     { CsmQueryOperator.Contains, Operator.Contains },
                                                                                     { CsmQueryOperator.Like, Operator.Like },
                                                                                     { CsmQueryOperator.NotLike, Operator.NotLike},
                                                                                     { CsmQueryOperator.GreaterThanEqual, Operator.GreaterThanEqual},
                                                                                     { CsmQueryOperator.LessThanEqual, Operator.LessThanEqual}
                                                                                     //We would like to be able to do these operations but trebuchet
                                                                                     //is currently throwing an error "Incorrect syntax near '@P1'"
                                                                                     //We will include these operators when we have a fix.
                                                                                     //{ CsmQueryOperator.StartsWith, Operator.BeginsWith },
                                                                                     //{ CsmQueryOperator.DoesNotContain, Operator.DoesNotContain }
                                                                                 };


        internal static QueryDef AndWhere(this QueryDef query, BusinessObjectDef businessObjectDef, string fieldName, CsmQueryOperator op)
        {
            var cherWellTypedValue = TypedValue.AlwaysBlank;
            return GetQuery(query, businessObjectDef, fieldName, op, cherWellTypedValue);
        }

        internal static QueryDef AndWhere(this QueryDef query, BusinessObjectDef businessObjectDef, string fieldName, CsmQueryOperator op, string value)
        {
            var cherWellTypedValue = TypedValue.ForString(value);
            return GetQuery(query, businessObjectDef, fieldName, op, cherWellTypedValue);
        }

        internal static QueryDef AndWhere(this QueryDef query, BusinessObjectDef businessObjectDef, string fieldName, CsmQueryOperator op, bool value)
        {
            var cherWellTypedValue = TypedValue.ForBool(value);
            return GetQuery(query, businessObjectDef, fieldName, op, cherWellTypedValue);
        }

        internal static QueryDef AndWhere(this QueryDef query, BusinessObjectDef businessObjectDef, string fieldName, CsmQueryOperator op, DateTime value)
        {
            var cherWellTypedValue = TypedValue.ForDateTime(value);
            return GetQuery(query, businessObjectDef, fieldName, op, cherWellTypedValue);
        }

        internal static QueryDef AndWhereRelated(this QueryDef query, BusinessObjectDef businessObjectDef, BusinessObjectDef relatedBusinessObjectDef, string relationshipName, string fieldName, CsmQueryOperator op, string value)
        {
            var cherWellOperator = OperatorMap[op];
            var cherWellTypedValue = TypedValue.ForString(value);

            var fieldDef = relatedBusinessObjectDef.Fields.GetFieldByDisplayNameOrName(fieldName);
            var relationshipDef = businessObjectDef.GetRelationshipDefByName(relationshipName);

            QueryConditionComparisonClause clauseAdditionalWhere = query.TopLevelGroupingClause.CreateFieldValueClause(fieldDef.FullId, cherWellOperator, cherWellTypedValue);
            QueryConditionClause clause = query.TopLevelGroupingClause.CreateRelatedClause(businessObjectDef.Id, relationshipDef.Id, RelatedBusObOccurrence.Any, 0, clauseAdditionalWhere);
            query.TopLevelGroupingClause.Clauses.Add(clause);

            return query;
        }

        internal static QueryDef OrWhere(this QueryDef query, BusinessObjectDef businessObjectDef, string fieldName, CsmQueryOperator op, string value)
        {
            query.AndWhere(businessObjectDef, fieldName, op, value);
            query.TopLevelGroupingClause.OrClauses = true;
            return query;
        }

        internal static QueryDef OrWhere(this QueryDef query, BusinessObjectDef businessObjectDef, string fieldName, CsmQueryOperator op, bool value)
        {
            query.AndWhere(businessObjectDef, fieldName, op, value);
            query.TopLevelGroupingClause.OrClauses = true;
            return query;
        }

        internal static QueryDef OrWhere(this QueryDef query, BusinessObjectDef businessObjectDef, string fieldName, CsmQueryOperator op, DateTime value)
        {
            query.AndWhere(businessObjectDef, fieldName, op, value);
            query.TopLevelGroupingClause.OrClauses = true;
            return query;
        }

        private static QueryDef GetQuery(QueryDef query, BusinessObjectDef businessObjectDef, string fieldName, CsmQueryOperator op, TypedValue value)
        {
            var cherWellOperator = OperatorMap[op];
            var fieldDef = businessObjectDef.Fields.GetFieldByDisplayNameOrName(fieldName);
            QueryConditionClause clause = query.TopLevelGroupingClause.CreateFieldValueClause(fieldDef.Id, cherWellOperator, value);
            query.TopLevelGroupingClause.Clauses.Add(clause);

            return query;
        }
    }
}
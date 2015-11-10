using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using CsmMagic.Attributes;
using CsmMagic.Exceptions;
using CsmMagic.Models;
using Trebuchet.API;

namespace CsmMagic
{
    /// <summary>
    /// Collection of static helper methods that interact with the Field and Relationship attributes defined on CsmModels
    /// </summary>
    internal static class FieldHelper
    {
        /// <summary>
        /// Attempts to retrieve the specified field by name, and throws an exception if it's not found
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="fieldName"></param>
        /// <param name="businessObjectTypeName"></param>
        /// <returns></returns>
        internal static FieldDef GetFieldByNameThrowIfNotFound(this FieldDefListProperty fields, string fieldName, string businessObjectTypeName)
        {
            var fieldDef = fields.GetFieldByDisplayNameOrName(fieldName);
            if (fieldDef == null)
            {
                throw new CherwellDataException(string.Format("Expected to find field with name {0} on Cherwell business object {1}, but it was not found. Ensure your CsmMagic models are in sync with your Cherwell blueprint.", fieldName, businessObjectTypeName));
            }
            return fieldDef;
        }

        /// <summary>
        /// Gets the field attribute from the member of T specified in the expression and retrieves its Cherwell field name
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static string GetCsmFieldNameFromMember<T>(Expression<Func<T, object>> expression)
        {
            var body = expression.Body;
            MemberInfo member;
            if (body is MemberExpression)
            {
                member = ((MemberExpression)expression.Body).Member;
            }
            else
            {
                var op = ((UnaryExpression)expression.Body).Operand;
                member = ((MemberExpression)op).Member;
            }

            var fieldAttribute = member.GetCustomAttribute<FieldAttribute>();
            if (fieldAttribute == null)
            {
                throw new InvalidOperationException(string.Format("Can only call this on class members which have been decorated with the {0} attribute.", typeof(FieldAttribute)));
            }

            return fieldAttribute.Name;
        }

        /// <summary>
        /// Gets the relationship attribute from the member of T specified in the expression and retrieves its Cherwell relationship name
        /// </summary>
        /// <param name="relatedPropertyExpression"></param>
        /// <returns></returns>
        internal static string GetCsmRelationshipNameFromMember<T>(Expression<Func<T, BusinessObjectModel>> relatedPropertyExpression)
        {
            return GetCsmRelationshipNameFromMember((MemberExpression)relatedPropertyExpression.Body);
        }

        /// <summary>
        /// Gets the relationship attribute from the member of T specified in the expression and retrieves its Cherwell relationship name
        /// </summary>
        /// <param name="relatedPropertyExpression"></param>
        /// <returns></returns>
        internal static string GetCsmRelationshipNameFromMember<T, TRelated>(Expression<Func<T, IEnumerable<TRelated>>> relatedPropertyExpression)
            where TRelated : BusinessObjectModel
        {
            return GetCsmRelationshipNameFromMember((MemberExpression)relatedPropertyExpression.Body);
        }

        /// <summary>
        /// Gets the relationship attribute from the member of T specified in the expression and retrieves its Cherwell relationship name
        /// </summary>
        /// <param name="relatedPropertyExpression"></param>
        /// <returns></returns>
        internal static string GetCsmRelationshipNameFromMember<T>(Expression<Func<T, object>> relatedPropertyExpression)
        {
            return GetCsmRelationshipNameFromMember((MemberExpression)relatedPropertyExpression.Body);
        }

        /// <summary>
        /// Gets the relationship attribute from the member of T specified in the expression and retrieves its Cherwell relationship name
        /// </summary>
        /// <param name="relatedPropertyExpression"></param>
        /// <returns></returns>
        internal static string GetCsmRelationshipNameFromMember(MemberExpression relatedPropertyExpression)
        {
            var relationshipAttribute = relatedPropertyExpression.Member.GetCustomAttribute<RelationshipAttribute>();
            if (relationshipAttribute == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Can only construct related type queries using class members which are navigational properties that have been decorated with the {0} attribute.",
                        typeof(RelationshipAttribute)));
            }
            return relationshipAttribute.RelationshipName;
        }

        internal static string GetCsmRelationshipNameFromMember<TOne, TTwo>(Expression<Func<TOne, TTwo>> relatedPropertyExpression)
        {
            return GetCsmRelationshipNameFromMember((MemberExpression)relatedPropertyExpression.Body);
        }
    }
}

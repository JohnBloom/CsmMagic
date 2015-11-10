using System;
using Trebuchet.API;
using System.Linq;
using CsmMagic.Exceptions;

namespace CsmMagic
{
    /// <summary>
    /// Extension methods on the Trebuchet API objects
    /// </summary>
    internal static class BusinessObjectExtensions
    {
        /// <summary>
        /// The GetRelationshipByName method in the Trebuchet DLLs doesn't always work consistently, it does not return all relationships by name.
        /// By digging into Trebuchet we learned that it sometimes tries to change the name that you input (such as removing a dash)
        /// and it no longer finds the relationship.  So, we want to search the Id that we want by name, then search trebuchet by Id.
        /// In summary, Cherwell shouldn't allow dashes in relationship names, but it does.
        /// This is just Cherwell being Cherwell.
        /// </summary>
        /// <typeparam name="TParentBO">The business object type representation of the parent data</typeparam>        
        /// <param name="parent">The business object instance representation of the parent data</param>
        /// <param name="relationshipName">The name of the relationship to target</param>
        /// <returns>The relationship specified by the relationshipName from the parent.</returns>
        internal static Relationship GetRelationshipByName<TParentBO>(this TParentBO parent, string relationshipName) where TParentBO : BusinessObject
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            if (string.IsNullOrWhiteSpace(relationshipName))
            {
                throw new ArgumentNullException("relationshipName");
            }

            var relationshipDef = parent.Def.Relationships.GetRelationshipByName(relationshipName);
            string relationshipId;

            if (relationshipDef == null)
            {
                var relationships = parent.Def.Relationships.Values;
                var relationship = relationships.Cast<RelationshipDef>().FirstOrDefault(rd => rd.Name == relationshipName);
                if (relationship == null)
                {
                    throw new CherwellDataException(string.Format("Could not find expected relationship with name {0} on type {1}", relationshipName, parent.Def.Name));
                }
                relationshipId = relationship.Id;
            }
            else
            {
                relationshipId = relationshipDef.Id;
            }
           
            return parent.GetRelationship(relationshipId);
        }

        internal static RelationshipDef GetRelationshipDefByName<TParentBO>(this TParentBO parentDef, string relationshipName) where TParentBO : BusinessObjectDef
        {
            var relationshipDef = parentDef.Relationships.GetRelationshipByName(relationshipName);

            if (relationshipDef == null)
            {
                var relationships = parentDef.Relationships.Values;
                relationshipDef = relationships.Cast<RelationshipDef>().FirstOrDefault(rd => rd.Name == relationshipName);
                if (relationshipDef == null)
                {
                    throw new CherwellDataException(string.Format("Could not find expected relationship with name {0} on type {1}", relationshipName, parentDef.Name));
                }
                return relationshipDef;
            }

            return relationshipDef;
        }
    }
}

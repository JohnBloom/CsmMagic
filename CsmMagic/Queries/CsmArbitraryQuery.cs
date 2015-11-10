using Trebuchet;
using Trebuchet.API;

namespace CsmMagic.Queries
{
    /// <summary>
    /// A fluent builder interface for creating a trebuchet query.
    /// Continue to extend the interface by adding more methods as required
    /// </summary>
    public class CsmArbitraryQuery : ICsmArbitraryQuery
    {
        /// <summary>
        /// This constructor creates a BusOb query for the Arbitrary Data Type
        /// </summary>
        public CsmArbitraryQuery()
        {
        }

        protected CsmArbitraryQuery(QueryDef query) : this()
        {
            TrebuchetQuery = query;
        }

        internal QueryDef TrebuchetQuery { get; set; }

        internal BusinessObjectDef TypeDefinition { get; set; }

        protected internal string TypeName { get; set; }

        /// <summary>
        /// Overwrites the query's business object definition with the provided definition
        /// </summary>
        /// <param name="businessObjectDefinitionName"></param>
        /// <returns></returns>
        public ICsmArbitraryQuery ForArbitraryBusinessObject(string businessObjectDefinitionName)
        {
            return ForType(businessObjectDefinitionName);
        }

        /// <summary>
        /// Returns the query (finishes the build)
        /// </summary>
        /// <returns></returns>
        internal QueryDef GetTrebuchetQuery()
        {
            return TrebuchetQuery;
        }

        protected ICsmArbitraryQuery ForType(string typeDefName)
        {
            TrebuchetQuery = QueryDef.CreateQuery();
            TypeDefinition = TrebuchetApi.Api.DefinitionRepository.GetBusObDefByNameOrId(typeDefName);
            TrebuchetQuery.BusObId = TypeDefinition.Id;
            TrebuchetQuery.QueryResultType = QueryResultType.BusOb;
            TypeName = typeDefName;
            return this;
        }
    }
}
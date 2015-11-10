namespace CsmMagic.Queries
{
    /// <summary>
    /// A fluent builder interface for creating a trebuchet query.
    /// Continue to extend the interface by adding more methods as required
    /// </summary>
    public interface ICsmArbitraryQuery
    {
        /// <summary>
        /// Overwrites the query's business object definition with the provided definition
        /// </summary>
        /// <param name="businessObjectDefinitionName"></param>
        /// <returns></returns>
        ICsmArbitraryQuery ForArbitraryBusinessObject(string businessObjectDefinitionName);
    }
}
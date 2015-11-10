using CsmMagic.Models;

namespace CsmMagic.Queries
{
    public class ParsedExpression
    {
        public string Property { get; set; }

        // TODO: We may need to support integer, bool, DateTime, etc. values in the future
        public string Value { get; set; }

        public CsmQueryOperator Operator { get; set; }

        public bool IsOr { get; set; }
    }
}

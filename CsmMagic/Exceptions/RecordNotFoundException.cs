using System;

namespace CsmMagic.Exceptions
{
    /// <summary>
    /// Describes an error caused by missing expected data
    /// </summary>
    public class RecordNotFoundException : ApplicationException
    {
        public RecordNotFoundException(string recId, string recordTypeName)
        {
            RecId = recId;
            RecordTypeName = recordTypeName;
        }

        protected RecordNotFoundException(string message)
            : base(message)
        {
        }

        public override string Message
        {
            get
            {
                if (!string.IsNullOrEmpty(RecId))
                {
                    return string.Format("Expected to find {0} with ID: {1}", RecordTypeName, RecId);
                }
                else
                {
                    return base.Message;
                }
            }
        }

        protected string RecId { get; private set; }

        protected string RecordTypeName { get; private set; }
    }
}
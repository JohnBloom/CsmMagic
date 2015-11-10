using System;

namespace CsmMagic.Exceptions
{
    /// <summary>
    /// Describes an error caused by missing expected one step objects
    /// </summary>
    public class OneStepNotFoundException : ApplicationException
    {
        public OneStepNotFoundException(string oneStepName, string businessObjectTypeName)
        {
            OneStepName = oneStepName;
            BusinessObjectTypeName = businessObjectTypeName;
        }

        protected OneStepNotFoundException(string message)
            : base(message)
        {
        }

        public override string Message
        {
            get
            {
                return !string.IsNullOrEmpty(this.OneStepName) ? string.Format("Expected to find one step named {0} for business object named {1}", this.OneStepName, this.BusinessObjectTypeName) : base.Message;
            }
        }

        protected string OneStepName { get; private set; }

        protected string BusinessObjectTypeName { get; private set; }
    }
}
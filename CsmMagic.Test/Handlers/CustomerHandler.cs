using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsmMagic.Handlers;

namespace CsmMagic.Test.Handlers
{
    /// <summary>
    /// Customer handler for customer. You can define what happens for CUD operations.
    /// </summary>
    public class CustomerHandler : BaseBusinessObjectHandler<TestCustomer>
    {
        public override void Create(TestCustomer incomingDomainObject, IHandlerClient client)
        {
            //Adding the [From Handler] to the name to show that we can manipulate the item being created.
            incomingDomainObject.Name = "[From Handler] " + incomingDomainObject.Name;
            client.Create(incomingDomainObject);
        }

        public override void Delete(TestCustomer entity, IHandlerClient client)
        {
            //You can use handlers to guard data
            if (entity.Name != "[From Handler] Test Customer")
            {
                throw new InvalidOperationException("Dont delete anything but test sample data!");
            }

            //You can use handlers to log data
            Debug.WriteLine("Customer " + entity.Name + " was deleted");

            client.Delete(entity);
        }

        public override void Update(TestCustomer incomingDomainObject, IHandlerClient client)
        {
            //You can use handlers for validation
            if (!incomingDomainObject.Phone.Contains("-"))
            {
                throw new Exception("Bad phone number input");
            }

            client.Update(incomingDomainObject);
        }
    }
}

using System.Linq;
using CsmMagic.Test.Models;
using CsmMagic.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsmMagic.Test
{
    [TestClass]
    public class UpdateTests
    {
        private ICsmClient _client;
        private string _defaultCustomerId = "93db6da57bde4b909d98d340d59e22c974abd9c903";
        private string _testIncidentName = "Test Incident";

        [TestInitialize]
        public void Setup()
        {
            //These tests are designed to run against the Cherwell Demo database
            //Make sure that your connection exists in C:\ProgramData\Trebuchet\Connections.xml
            var config = new CsmClientConfiguration("CSDAdmin", "CSDAdmin", "[common]devlocal");

            //The trebuchet dlls maintain the connection to the server. 
            var factory = new CsmClientFactory(config);

            _client = factory.GetCsmClient();
        }

        /// <summary>
        /// If you would like to update a record just set the values and call update. This will persist all of the property values in the 
        /// object to Cherwell.
        /// </summary>
        [TestMethod]
        public void UpdateIncident_Description_Test()
        {
            var incident = new TestIncident
            {
                CustomerId = _defaultCustomerId,
                Title = _testIncidentName,
                Description = "Test Test Test"
            };

            _client.Create(incident);

            Assert.IsNotNull(incident.RecId);
            Assert.AreEqual("Test Test Test", incident.Description);

            incident.Description = "Test Updated Description";

            _client.Update(incident);

            var incidentQuery = _client.GetQuery<TestIncident>().ForRecId(incident.RecId);
            var sameIncident = _client.ExecuteQuery(incidentQuery);

            Assert.AreEqual("Test Updated Description", sameIncident.First().Description);

            _client.Delete(incident);
        }

        /// <summary>
        /// If you would like to update a record just set the values and call update. This will persist all of the property values in the 
        /// object to Cherwell.
        /// </summary>
        [TestMethod]
        public void UpdateCustomer_WithValidation_Test()
        {
            var customer = new TestCustomer()
            {
                Name = "Test Customer"
            };

            _client.Create(customer);

            Assert.IsNotNull(customer.RecId);
            Assert.AreEqual("[From Handler] Test Customer", customer.Name);

            customer.Phone = "7854454544";

            try
            {
                _client.Update(customer);
            }
            catch
            {
                //update phone and keep going
                customer.Phone = "785-445-4544";
            }

            _client.Update(customer);

            var customerQuery = _client.GetQuery<TestCustomer>().ForRecId(customer.RecId);
            var sameCustomer = _client.ExecuteQuery(customerQuery);

            Assert.AreEqual("785-445-4544", sameCustomer.First().Phone);

            _client.Delete(customer);
        }

        /// <summary>
        /// When you need to call custom validation on a field such that the field will not be updated use a validator
        /// The validators are a per-field property that can determine whether the value is persisted to the db, ignored, or 
        /// whether an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void UpdateIncident_WithValidatorValidation_Test()
        {
            var incident = new TestIncident
            {
                CustomerId = _defaultCustomerId,
                Title = _testIncidentName,
                Description = "Test Test Test"
            };

            _client.Create(incident);

            Assert.IsNotNull(incident.RecId);
            Assert.AreEqual("Test Test Test", incident.Description);

            incident.Title = "throw exception";
            var threwException = false;

            try
            {
                _client.Update(incident);
            }
            catch (CsmValidationException e)
            {
                threwException = true;
            }
            
            Assert.IsTrue(threwException);
            _client.Delete(incident);
        }

        [TestMethod]
        public void UpdateIncident_UpdateFails_UpdateIsRolledBack()
        {
            var testTitle = "Test Incident 2";
            var incident = new TestIncident
            {
                CustomerId = _defaultCustomerId,
                Title = testTitle,
                Description = "Test Test Test"
            };

            _client.Create(incident);

            Assert.IsNotNull(incident.RecId);
            Assert.AreEqual("Test Test Test", incident.Description);

            incident.Description = "Changed Test";
            incident.Title = "throw exception";

            try
            {
                _client.Update(incident);
            }
            catch
            {
                // Swallow the exception so we can confirm that our updates were rolled back
            }

            var readIncident =
                _client.ExecuteQuery(_client.GetQuery<TestIncident>().Where(x => x.Title == testTitle)).Single();
            Assert.AreEqual("Test Test Test", readIncident.Description);
            _client.Delete(incident);
        }

    }
}

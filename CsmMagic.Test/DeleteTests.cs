using System;
using System.Linq;
using CsmMagic.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsmMagic.Test
{
    [TestClass]
    public class DeleteTests
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

        [TestCleanup]
        public void Cleanup()
        {
            //Remove all of the test incidents so the query tests pass
            var incidentQuery = _client.GetQuery<TestIncident>().Where(x => x.Title == "Test Incident");
            var incidents = _client.ExecuteQuery(incidentQuery);

            foreach (var testIncident in incidents)
            {
                _client.Delete(testIncident);
            }
        }

        /// <summary>
        /// Calling delete on an object that has an Id from cherwell will remove the object
        /// </summary>
        [TestMethod]
        public void DeleteIncident_Test()
        {
            const string testString = "Test Test Test";

            var incident = new TestIncident
            {
                CustomerId = _defaultCustomerId,
                Title = _testIncidentName,
                Description = testString
            };

            _client.Create(incident);

            Assert.IsNotNull(incident.RecId);

            _client.Delete(incident);

            var incidentQuery = _client.GetQuery<TestIncident>().ForRecId(incident.RecId);
            var incidents = _client.ExecuteQuery(incidentQuery);

            Assert.AreEqual(0, incidents.Count());
        }

        /// <summary>
        /// In this case inorder to guard against corrupting the test data we block any calls to 
        /// delete a customer that was not created through the tests. This is done in the handlers.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeleteCustomer_WithHandler_Test()
        {
            var customerQuery = _client.GetQuery<TestCustomer>().ForRecId(_defaultCustomerId);
            var customer = _client.ExecuteQuery(customerQuery).First();

            _client.Delete(customer);
        }
    }
}

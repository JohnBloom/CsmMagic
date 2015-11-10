using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsmMagic.Models;
using CsmMagic.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsmMagic.Test
{
    [TestClass]
    public class ReadTests
    {
        private ICsmClient _client;
        private string _testIncidentName = "Test Incident";
        private string _defaultCustomerId = "93db6da57bde4b909d98d340d59e22c974abd9c903";
        private readonly List<Attachment> _tmpFileList = new List<Attachment>()
        {
            new Attachment()
            {
                InputAttachment = new FileInfo(Path.GetTempFileName())
            },
            new Attachment()
            {
                InputAttachment = new FileInfo(Path.GetTempFileName())
            }
        }; 

        [TestInitialize]
        public void Setup()
        {
            // These tests are designed to run against the Cherwell Demo database
            // Make sure that your connection exists in C:\ProgramData\Trebuchet\Connections.xml
            var config = new CsmClientConfiguration("CSDAdmin", "CSDAdmin", "[common]devlocal");

            // The trebuchet dlls maintain the connection to the server. 
            var factory = new CsmClientFactory(config);

            _client = factory.GetCsmClient();
            
            // Put some meat in each of those temp files so we have some bytes to transfer
            foreach (var tmpFile in _tmpFileList)
            {
                try
                {
                    var streamWriter = tmpFile.InputAttachment.AppendText();
                    streamWriter.WriteLine(string.Format("Test Attachment: {0}", tmpFile.InputAttachment.FullName));
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Creating files for Read Tests");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Remove all of the test incidents so the query tests pass
            var incidentQuery = _client.GetQuery<TestIncident>().Where(x => x.Title == "Test Incident");
            var incidents = _client.ExecuteQuery(incidentQuery);

            foreach (var testIncident in incidents)
            {
                _client.Delete(testIncident);
            }
            
            // Delete the tmpfiles from machine
            foreach (var tmpFile in _tmpFileList)
            {
                try
                {
                    if (tmpFile.InputAttachment.Exists)
                    {
                        tmpFile.InputAttachment.Delete();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Deleting files for Read Tests");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Loading all of a certain record type is really simple
        /// Just build the query with your type and execute it
        /// </summary>
        [TestMethod]
        public void BasicQuery_All_Test()
        {
            var customerQuery = _client.GetQuery<TestCustomer>();
            var customers = _client.ExecuteQuery(customerQuery);

            Assert.AreEqual(45, customers.Count());
        }

        /// <summary>
        /// You can load a specific record by Id. This has some optimizations 
        /// under the hood so it is better to use than an equals query.
        /// </summary>
        [TestMethod]
        public void BasicQuery_ForRecId_Test()
        {
            const string defaultCustomerId = "93db6da57bde4b909d98d340d59e22c974abd9c903";
            var customerQuery = _client.GetQuery<TestCustomer>().ForRecId(defaultCustomerId);
            var customer = _client.ExecuteQuery(customerQuery);

            Assert.AreEqual("Default Customer", customer.First().Name);
        }

        /// <summary>
        /// If you need to get a property that is equal to a certain value
        /// This query uses expression syntax, which is the most natural way to write a query in CsmMagic.
        /// Not all use cases have been handled by the expression syntax but we would like to move
        /// all of the queries to it eventually. So if it can be written in the expession syntax do it.
        /// All of the uses of this syntax are demoed in the following tests.
        /// </summary>
        [TestMethod]
        public void BasicQuery_Equal_Test()
        {
            var customerQuery = _client.GetQuery<TestCustomer>().Where(x => x.Name == "Default Customer");
            var customers = _client.ExecuteQuery(customerQuery);

            Assert.AreEqual(1, customers.Count());
        }

        /// <summary>
        /// Since you know you are getting back a single record you can include that in the query.
        /// This will optimize the query much like ForRecId does.
        /// </summary>
        [TestMethod]
        public void BasicQuery_EqualForSingleRecord_Test()
        {
            var customerQuery = _client.GetQuery<TestCustomer>().ForSingleRecord().Where(x => x.Name == "Default Customer");
            var customers = _client.ExecuteQuery(customerQuery);

            Assert.AreEqual(1, customers.Count());
        }

        /// <summary>
        /// If you need to get a property that is less than a certain value using the expression syntax
        /// </summary>
        [TestMethod]
        public void BasicQuery_NotEqual_Test()
        {
            var customerQuery = _client.GetQuery<TestCustomer>().Where(x => x.Name != "Default Customer");
            var customers = _client.ExecuteQuery(customerQuery);

            Assert.AreEqual(44, customers.Count());
        }

        /// <summary>
        /// If you need to get a property that is less than a certain value using the expression syntax
        /// </summary>
        [TestMethod]
        public void BasicQuery_LessThan_Test()
        {
            var incidentQuery = _client.GetQuery<TestIncident>().Where(x => x.DurationInHours < 24);
            var incidents = _client.ExecuteQuery(incidentQuery);

            Assert.AreEqual(37, incidents.Count());
        }

        /// <summary>
        /// If you need to get a property that is greater than a certain value using the expression syntax
        /// </summary>
        [TestMethod]
        public void BasicQuery_GreaterThan_Test()
        {
            var incidentQuery = _client.GetQuery<TestIncident>().Where(x => x.DurationInHours > 24);
            var incidents = _client.ExecuteQuery(incidentQuery);

            Assert.AreEqual(22, incidents.Count());
        }

        /// <summary>
        /// If you need to get a property that is greater than or equal to a certain value using the expression syntax
        /// </summary>
        [TestMethod]
        public void BasicQuery_GreaterThanEqual_Test()
        {
            var incidentQuery = _client.GetQuery<TestIncident>().Where(x => x.DurationInHours >= 24);
            var incidents = _client.ExecuteQuery(incidentQuery);

            Assert.AreEqual(22, incidents.Count());
        }

        /// <summary>
        /// If you need to get a property that is less than or equal to a certain value using the expression syntax
        /// </summary>
        [TestMethod]
        public void BasicQuery_LessThanEqual_Test()
        {
            var incidentQuery = _client.GetQuery<TestIncident>().Where(x => x.DurationInHours <= 24);
            var incidents = _client.ExecuteQuery(incidentQuery);

            Assert.AreEqual(37, incidents.Count());
        }

        /// <summary>
        /// If you would like to find a string that is contained in a certain field
        /// There is no expression syntax that can handle this query so you have to resort to passing the 
        /// parameters individually
        /// </summary>
        [TestMethod]
        public void BasicQuery_Contains_Test()
        {
            var customerQuery = _client.GetQuery<TestCustomer>().Where(x => x.Name, CsmQueryOperator.Contains, "Max");
            var customers = _client.ExecuteQuery(customerQuery);

            Assert.AreEqual(1, customers.Count());
        }

        /// <summary>
        /// If you would like to use pattern matching to get a subset of the customers like a certain string
        /// </summary>
        [TestMethod]
        public void BasicQuery_Like_Test()
        {
            var customerQuery = _client.GetQuery<TestCustomer>().Where(x => x.Name, CsmQueryOperator.Like, "T%");
            var customers = _client.ExecuteQuery(customerQuery);

            Assert.AreEqual(4, customers.Count());
        }

        /// <summary>
        /// If you would like to use pattern matching to get a subset of the customers not like a certain string
        /// </summary>
        [TestMethod]
        public void BasicQuery_NotLike_Test()
        {
            var customerQuery = _client.GetQuery<TestCustomer>().Where(x => x.Name, CsmQueryOperator.NotLike, "T%");
            var customers = _client.ExecuteQuery(customerQuery);

            Assert.AreEqual(41, customers.Count());
        }

        /// <summary>
        /// You can combine multiple predicates into a single query
        /// </summary>
        [TestMethod]
        public void BasicQuery_CombineOrPredicates_Test()
        {
            var defaultCustomerRecId = "93db6da57bde4b909d98d340d59e22c974abd9c903";
            var customerQuery = _client.GetQuery<TestCustomer>().Where(x => x.Name == "Max Megalos" || x.RecId == defaultCustomerRecId);
            var customers = _client.ExecuteQuery(customerQuery);

            // This should retrieve both the customer with name "Max Megalos" and the customer with the default customer rec ID (which is not Max Megalos)
            Assert.AreEqual(2, customers.Count());
        }

        /// <summary>
        /// You can combine multiple predicates into a single query
        /// </summary>
        [TestMethod]
        public void BasicQuery_CombineAndPredicates_Test()
        {
            var defaultCustomerRecId = "93db6da57bde4b909d98d340d59e22c974abd9c903";
            var customerQuery = _client.GetQuery<TestCustomer>().Where(x => x.Name == "Default Customer" && x.RecId == defaultCustomerRecId);
            var customers = _client.ExecuteQuery(customerQuery);

            // This should only the default customer (as it fulfills both the predicates)
            Assert.AreEqual(1, customers.Count());
        }

        /// <summary>
        /// If you would like to load a customer and all of their incidents you can use Include() on a relationship
        /// The relationship needs to be setup in the model as an IEnumerable<T> and have the [Relationship] tag.
        /// You can see an example in the TestCustomer model.
        /// </summary>
        [TestMethod]
        public void IncludeQuery_WithEqual_Test()
        {
            var customerQuery = _client.GetQuery<TestCustomer>().Include(x => x.Incidents).Where(x => x.Name == "Max Megalos");
            var customers = _client.ExecuteQuery(customerQuery);

            Assert.AreEqual(2, customers.First().Incidents.Count());
        }

        /// <summary>
        /// If you already have a customer and want to get its incidents
        /// </summary>
        [TestMethod]
        public void ForChildernQuery_GetIncidentsForSpecificCustomer_Test()
        {
            var customerQuery = _client.GetQuery<TestCustomer>().ForSingleRecord().Where(x => x.Name == "Max Megalos");
            var customer = _client.ExecuteQuery(customerQuery).First();

            var incidentsQuery = _client.GetQuery<TestCustomer>().ForChildren(x => x.Incidents, customer.RecId);
            var incidents = _client.ExecuteQuery(incidentsQuery);

            Assert.AreEqual(2, incidents.Count());
        }

        /// <summary>
        /// If you want to query using a model with nullable fields
        /// </summary>
        [TestMethod]
        public void BasicQuery_NullableField_Test()
        {
            var defaultIncidentRecId = "93fd44e7cfb25a12f9e48c4a66a117a5ed0ccfd9af";
            var incidentQuery = _client.GetQuery<NullableFieldIncident>().ForRecId(defaultIncidentRecId);
            var incidents = _client.ExecuteQuery(incidentQuery);

            Assert.AreEqual(1, incidents.Count());
        }

        /// <summary>
        /// If you want to query using a model with nullable fields
        /// </summary>
        [TestMethod]
        public void BasicQuery_KeyQuery_Test()
        {
            var defaultIncidentRecId = "93d605e29a4a79c71cb4584dc1b5d740a1edeea675";
            var incidentId = "100576";

            var incidentQuery = _client.GetQuery<TestIncident>().ForKey(incidentId);
            var incidents = _client.ExecuteQuery(incidentQuery);
            var incident = incidents.First();

            var recIncidentQuery = _client.GetQuery<TestIncident>().ForKey(defaultIncidentRecId);
            var recIncidents = _client.ExecuteQuery(recIncidentQuery);
            var recIncident = recIncidents.First();

            Assert.IsNotNull(incident);
            Assert.IsNotNull(recIncident);
            Assert.AreEqual(defaultIncidentRecId, incident.RecId);
            Assert.AreEqual(incidentId, incident.IncidentID);
        }

        [TestMethod]
        public void GetAttachment_Test()
        {
            const string testString = "Test Test Test";
            var incident = new TestIncident
            {
                CustomerId = _defaultCustomerId,
                Title = _testIncidentName,
                Description = testString
            };

            _client.Create(incident);

            _client.AttachFileToObject(incident, _tmpFileList);

            var streams = _client.GetAttachmentsFromObject(incident);
            
            Assert.AreEqual(_client.HasAttachments(incident), streams.Count());

        }
    }
}

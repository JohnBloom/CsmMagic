using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsmMagic.Exceptions;
using CsmMagic.Models;
using CsmMagic.Test.Models;
using CsmMagic.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsmMagic.Test
{
    [TestClass]
    public class CreateTests
    {
        private ICsmClient _client;
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
        private string _defaultCustomerId = "93db6da57bde4b909d98d340d59e22c974abd9c903";
        private string _testCustomerName = "Test Customer";
        private string _testIncidentName = "Test Incident";

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
            foreach (var attachment in _tmpFileList)
            {
                try
                {
                    var streamWriter = attachment.InputAttachment.AppendText();
                    streamWriter.WriteLine(string.Format("Test Attachment: {0}", attachment.InputAttachment.FullName));
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Creating files for Create Tests");
                    Console.WriteLine(ex.StackTrace);
                }
            }
           
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Remove all of the test customers so the query tests pass
            var handlerTitle = "[From Handler] Test Customer";
            var cusQuery = _client.GetQuery<TestCustomer>().Where(x => x.Name == handlerTitle);
            var customers = _client.ExecuteQuery(cusQuery);
            
            foreach (var testCustomer in customers)
            {
                _client.Delete(testCustomer);
            }

            // Remove all of the test incidents so the query tests pass
            var incidentQuery = _client.GetQuery<TestIncident>().Where(x => x.Title == "Test Incident");
            var incidents = _client.ExecuteQuery(incidentQuery);

            foreach (var testIncident in incidents)
            {
                _client.Delete(testIncident);
            }

            // Delete the tmpfiles from machine
            foreach (var attachment in _tmpFileList)
            {
                try
                {
                    if (attachment.InputAttachment.Exists)
                    {
                        attachment.InputAttachment.Delete();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error deleting files for Create Tests");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// If you dont provide the create call with all of the required fields you will get an exception 
        /// telling you why the record was not able to be created.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CherwellUpdateException))]
        public void CreateIncident_WithNoCustomer_Test()
        {
            var incident = new TestIncident();

            _client.Create(incident);
        }

        /// <summary>
        /// Creating an incident with all of the required fields set will create the object in cherwell
        /// We know this is the case because the RecId has been filled in with a Cherwell generated key.
        /// </summary>
        [TestMethod]
        public void CreateIncident_WithDefaultCustomer_Test()
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
        }

        /// <summary>
        /// Attach file to cherwell
        /// </summary>
        [TestMethod]
        public void CreateAttachment_Test()
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
           
            Assert.AreEqual(_client.HasAttachments(incident), _tmpFileList.Count);
        }


        /// <summary>
        /// Creating an customer with all of the fields set will create the object in cherwell
        /// Notice that the name contains the string [From Handler]. This is because the handler
        /// is intercepting the create call for the Customer. This allows you to customize what 
        /// happens during the create for the specific type.
        /// </summary>
        [TestMethod]
        public void CreateCustomer_Test()
        {
            var customer = new TestCustomer()
            {
                Name = _testCustomerName
            };

            _client.Create(customer);

            Assert.IsNotNull(customer.RecId);
            Assert.AreEqual("[From Handler] Test Customer", customer.Name);
        }

        [TestMethod]
        public void CreateCustomer_CreateTransactionFails_CustomerIsDeleted()
        {
            // This will trip the field validation and cause the transaction to fail
            const string customerName = "Fail Transaction Name";
            var customer = new TestCustomer
            {
                Name = customerName
            };

            try
            {
                _client.Create(customer);
            }
            catch
            {
                // Swallow the exception to let us assert the rollback worked
            }

            var readFromCherwell = _client.ExecuteQuery(_client.GetQuery<TestCustomer>().Where(x => x.Name == customerName)).FirstOrDefault();
            Assert.IsNull(readFromCherwell);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Table;

namespace MvcWebRole1.Controllers
{
    //[Authorize]
    public class TableController : ApiController
    {
        private Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount;
        private CloudTableClient client;
        private CloudTable table;

        public TableController()
        {
            // Retrieve storage account from connection string 
            storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            // Create the Table client  
            client = storageAccount.CreateCloudTableClient();
            // Retrieve a reference to a table  
            table = client.GetTableReference("People");
            // Create the table if it doesn't already exist 
            table.CreateIfNotExists();
        }

        // GET api/Table
        public CustomerEntity Get()
        {
            var list = QueryAll();
            if (!list.Any())
            {
                Add();
            }

            return list.FirstOrDefault();
        }

 
        private void Add()
        {
            CustomerEntity customer1 = new CustomerEntity("Smith", "Jeff");
            customer1.Email = "Jeff@contoso.com";
            customer1.PhoneNumber = "425-555-0104";

            TableOperation insertOpt = TableOperation.Insert(customer1);
            table.Execute(insertOpt);
        }

        private void BatchAdd()
        {
            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            // Create a customer entity and add it to the table.
            CustomerEntity customer1 = new CustomerEntity("Smith", "Jeff");
            customer1.Email = "Jeff@contoso.com";
            customer1.PhoneNumber = "425-555-0104";

            // Create another customer entity and add it to the table.
            CustomerEntity customer2 = new CustomerEntity("Smith", "Ben");
            customer2.Email = "Ben@contoso.com";
            customer2.PhoneNumber = "425-555-0102";

            // Add both customer entities to the batch insert operation.
            batchOperation.Insert(customer1);
            batchOperation.Insert(customer2);

            // Execute the batch operation.
            table.ExecuteBatch(batchOperation);
        }

        /// <summary>
        /// Retrieve all entities in a partition
        /// </summary>
        private IEnumerable<CustomerEntity> QueryAll()
        {
            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<CustomerEntity> query =
                new TableQuery<CustomerEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Smith"));

            return table.ExecuteQuery(query);
        }

        /// <summary>
        /// Retrieve a range of entities in a partition
        /// </summary>
        private IEnumerable<CustomerEntity> QueryAll2()
        {
            // Create the table query.
            TableQuery<CustomerEntity> rangeQuery = new TableQuery<CustomerEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Smith"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, "E")));

            return table.ExecuteQuery(rangeQuery);
        }

        /// <summary>
        /// Retrieve a single entity
        /// </summary>
        private CustomerEntity QuerySingle()
        {
            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<CustomerEntity>("Smith", "Ben");

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Print the phone number of the result.
            if (retrievedResult.Result != null)
                return (CustomerEntity) retrievedResult.Result;
            else
                return null;
        }

        /// <summary>
        /// Query a subset of entity properties
        /// </summary>
        private IEnumerable<string> QuerySubSet()
        {
            // Define the query, and only select the Email property
            TableQuery<DynamicTableEntity> projectionQuery = new TableQuery<DynamicTableEntity>().Select(new string[] { "Email" });

            // Define an entity resolver to work with the entity after retrieval.
            EntityResolver<string> resolver = (pk, rk, ts, props, etag) => props.ContainsKey("Email") ? props["Email"].StringValue : null;

            return table.ExecuteQuery(projectionQuery, resolver, null, null);
        }

        /// <summary>
        /// Replace an entity
        /// </summary>
        private void Update()
        {
            // Assign the result to a CustomerEntity object.
            CustomerEntity updateEntity = QuerySingle();

            if (updateEntity != null)
            {
                // Change the phone number.
                updateEntity.PhoneNumber = "425-555-0105";

                // Create the InsertOrReplace TableOperation
                TableOperation updateOperation = TableOperation.Replace(updateEntity);

                /*
                 Replace operations will fail if the entity has been changed since it was retrieved from the server. 
                 * Furthermore, you must retrieve the entity from the server first in order for the Replace to be successful. 
                 * Sometimes, however, you don't know if the entity exists on the server and the current values stored in it are irrelevant - 
                 * your update should overwrite them all. To accomplish this, you would use an InsertOrReplace operation.
                 */
                //TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(updateEntity);

                // Execute the operation.
                table.Execute(updateOperation);
            }
        }

        private void Delete()
        {
            // Assign the result to a CustomerEntity object.
            CustomerEntity deleteEntity = QuerySingle();

            // Create the Delete TableOperation.
            if (deleteEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                // Execute the operation.
                table.Execute(deleteOperation);
               
            }

        }

        private void DeleteTable()
        {
            //Create the CloudTable that represents the "people" table.
            CloudTable table = client.GetTableReference("people");

            // Delete the table it if exists.
            table.DeleteIfExists();
        }

        public class CustomerEntity : TableEntity
        {
            public CustomerEntity(string lastName, string firstName)
            {
                this.PartitionKey = lastName;
                this.RowKey = firstName;
            }

            public CustomerEntity() { }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }
       
    }
}
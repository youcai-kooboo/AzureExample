using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MvcWebRole1.Controllers
{
    //[Authorize]
    public class QueueController : ApiController
    {
        private Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount;
        private CloudQueueClient queueClient;
        private CloudQueue queue;
 
        public QueueController()
        {
            // Retrieve storage account from connection string 
            storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the queue client.
            queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            queue = queueClient.GetQueueReference("myqueue");

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExists();
        }

        // GET api/Blob
        public string Get()
        {
            // Peek at the next message
            var message = queue.PeekMessage();
            if (message == null)
            {
                 message =  Create();
            }

            return message.AsString;
        }

        private CloudQueueMessage Create()
        {
            // Create a message and add it to the queue.
            var message = new CloudQueueMessage("Hello, World");
            queue.AddMessage(message);

            return message;
        }

        private void Update()
        {
            // Get the message from the queue and update the message contents.
            CloudQueueMessage message = queue.GetMessage();
            message.SetMessageContent("Updated contents.");
            queue.UpdateMessage(message,
                TimeSpan.FromSeconds(0.0),  // Make it visible immediately.
                MessageUpdateFields.Content | MessageUpdateFields.Visibility);
        }

        private void Delete()
        {
            // Get the next message
            CloudQueueMessage retrievedMessage = queue.GetMessage();

            //Process the message in less than 30 seconds, and then delete the message
            queue.DeleteMessage(retrievedMessage);
        }

        private void DeleteMessages()
        {
            foreach (CloudQueueMessage message in queue.GetMessages(20, TimeSpan.FromMinutes(5)))
            {
                // Process all messages in less than 5 minutes, deleting each message after processing.
                queue.DeleteMessage(message);
            }
        }

        /// <summary>
        /// Get the queue length
        /// </summary>
        /// <returns></returns>
        private int? GetMessageCount()
        {
            // Fetch the queue attributes.
            queue.FetchAttributes();

            // Retrieve the cached approximate message count.
            int? cachedMessageCount = queue.ApproximateMessageCount;

            return cachedMessageCount;
        }

        private void DeleteQueue()
        {
            // Retrieve a reference to a queue.
            CloudQueue queueTemp = queueClient.GetQueueReference("myqueue");

            // Delete the queue.
            queueTemp.Delete();
        }

    }
}
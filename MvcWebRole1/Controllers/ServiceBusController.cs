using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace MvcWebRole1.Controllers
{
    public class ServiceBusController : ApiController
    {
        private NamespaceManager namespaceManager;
        private string connectionString;
        private QueueClient Client;
         

        public ServiceBusController()
        {
            // Create the queue if it does not exist already
            connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists("TestQueue"))
            {
                namespaceManager.CreateQueue("TestQueue");
            }
            Client = QueueClient.CreateFromConnectionString(connectionString, "TestQueue");

            // Configure Queue Settings
            var qd = new QueueDescription("TestQueue");
            qd.MaxSizeInMegabytes = 5120;
            qd.DefaultMessageTimeToLive = new TimeSpan(0, 1, 0);

        }
 
        public void Get()
        {
            Create();
            //Client.Receive();

            // Continuously process messages sent to the "TestQueue" 
            for (int i = 0; i < 5; i++)
            {
                BrokeredMessage message = Client.Receive();

                if (message != null)
                {
                    try
                    {
                        Console.WriteLine("Body: " + message.GetBody<string>());
                        Console.WriteLine("MessageID: " + message.MessageId);
                        Console.WriteLine("Test Property: " + message.Properties["TestProperty"]);

                        // Remove message from queue
                        message.Complete();
                    }
                    catch (Exception)
                    {
                        // Indicate a problem, unlock message in queue
                        message.Abandon();
                    }
                }
            } 
        }

        private void Create()
        {
            for (int i = 0; i < 5; i++)
            {
                // Create message, passing a string message for the body
                BrokeredMessage message = new BrokeredMessage("Test message " + i);

                // Set some addtional custom app-specific properties
                message.Properties["TestProperty"] = "TestValue";
                message.Properties["Message number"] = i;

                // Send message to the queue
                Client.Send(message);
            }
        }

    }
}

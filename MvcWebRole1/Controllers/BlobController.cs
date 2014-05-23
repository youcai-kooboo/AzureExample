using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;
using BlobContainerPermissions = Microsoft.WindowsAzure.Storage.Blob.BlobContainerPermissions;
using BlobContainerPublicAccessType = Microsoft.WindowsAzure.Storage.Blob.BlobContainerPublicAccessType;
using CloudBlobClient = Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient;
using CloudBlobContainer = Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer;

namespace MvcWebRole1.Controllers
{
    //[Authorize]
    public class BlobController : ApiController
    {
        private Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private CloudBlobContainer container;
        private CloudBlobDirectory directory;
        private CloudBlockBlob blob;
        public BlobController()
        {
            // Retrieve storage account from connection string 
            storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            // Create the blob client  
            blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container  
            container = blobClient.GetContainerReference("mycontainer");
            // Create the container if it doesn't already exist 
            container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });

            // Retrieve reference to a BlobDirectory
            directory = container.GetDirectoryReference("photos/2014/sports");
            
        }

        // GET api/Blob
        public string Get()
        {
            if (!container.ListBlobs().Any())
            {
                blob = directory.GetBlockBlobReference("00");
                Upload(blob, AppDomain.CurrentDomain.BaseDirectory + @"/Images/orderedList0.png");
            }

            var sb = new StringBuilder();

            // Loop over items within the container and output the length and URI.
            LoopBlobItems(container.ListBlobs(), sb);

            return sb.ToString();
        }

        private void Upload(CloudBlockBlob cbb, string file)
        {
            // Create or overwrite blob with contents from a local file 
            using (var fileStream = System.IO.File.OpenRead(file))
            {
                cbb.UploadFromStream(fileStream);
            }
        }
 
        private void Delete()
        {
            var directoryTemp = container.GetDirectoryReference("photos/2014/temp");
            var blobTemp = directoryTemp.GetBlockBlobReference("01");
            Upload(blobTemp, AppDomain.CurrentDomain.BaseDirectory + @"/Images/orderedList1.png");

           
            blobTemp.Delete();
        }


        public void Download()
        {
            var blob = directory.GetBlockBlobReference("00");
 
            // Save blob contents to a file.
            using (var fileStream = File.OpenWrite(AppDomain.CurrentDomain.BaseDirectory + @"/Images/"+DateTime.Now+".png"))
            {
                blob.DownloadToStream(fileStream);
            }

            /* 
            string text;
            using (var memoryStream = new MemoryStream())
            {
                blockBlob2.DownloadToStream(memoryStream);
                text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            }*/

        }

        private void LoopBlobItems(IEnumerable<IListBlobItem> items, StringBuilder sb)
        {
            foreach (IListBlobItem item in items)
            {
                if (item is CloudBlockBlob)
                {
                    var blob = (CloudBlockBlob)item;
                    sb.AppendLine(String.Format("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri));
                }
                else if (item is CloudPageBlob)
                {
                    var pageBlob = (CloudPageBlob)item;
                    sb.AppendLine(String.Format("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri));
                }
                else if (item is CloudBlobDirectory)
                {
                    var directory = (CloudBlobDirectory)item;
                    sb.AppendLine(String.Format("Directory: {0}", directory.Uri));
                    LoopBlobItems(directory.ListBlobs(), sb);
                }
            }
        }

    }
}
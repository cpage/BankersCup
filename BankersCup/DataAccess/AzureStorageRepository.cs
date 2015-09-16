using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace BankersCup.DataAccess
{
    public class AzureStorageRepository
    {
        const string IMAGE_CONTAINER_NAME = "game{0}";
        static CloudBlobClient client;

        static AzureStorageRepository()
        {
            var storageAcct = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["GolfCupCloudStorage"].ConnectionString);

            client = storageAcct.CreateCloudBlobClient();
        }


        public static void SaveImage(int gameId, string imageName, byte[] image)
        {
            var container = client.GetContainerReference(string.Format(IMAGE_CONTAINER_NAME, gameId));
            
            
            container.CreateIfNotExists(BlobContainerPublicAccessType.Container);

            var imageBlob = container.GetBlockBlobReference(imageName);
            imageBlob.UploadFromByteArray(image, 0, image.Length);


        }

    }
}
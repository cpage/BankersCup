using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
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


        public static async Task<string> SaveImageAsync(int gameId, string imageName, byte[] image)
        {
            var container = client.GetContainerReference(string.Format(IMAGE_CONTAINER_NAME, gameId));
            
            
            await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Container, null, null);
            
                
            var imageBlob = container.GetBlockBlobReference(imageName);
            await imageBlob.UploadFromByteArrayAsync(image, 0, image.Length);
            return imageBlob.Uri.AbsoluteUri;


        }

    }
}
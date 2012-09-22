using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BPlustTree.Azure
{
    public class Azure_Blob_Utilities
    {
        int timeout = 60;

        public void Resize_Blob(CloudPageBlob pageBlob, CloudBlobClient blobStorage, long newBlobSize)
        {
            Uri requestUri = pageBlob.Uri;
            if (blobStorage.Credentials.NeedsTransformUri)
            {
                requestUri = new Uri(blobStorage.Credentials.TransformUri(requestUri.ToString()));
            }

            HttpWebRequest request = BlobRequest.SetProperties(requestUri, timeout,
                           pageBlob.Properties, null, newBlobSize);
            request.Timeout = timeout;
            blobStorage.Credentials.SignRequest(request);
            using (WebResponse response = request.GetResponse())
            {
                // call succeeded
            }; 
 
        }
    }
}

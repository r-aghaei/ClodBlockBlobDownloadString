# Download unicode text from Azure Blob without BOM

When you download a text content using `CloudBlockBlob.DownloadText()` if the blob contains the [BOM](https://en.wikipedia.org/wiki/Byte_order_mark) or byte order mark, then the returned text will contain some additional characters and if you use the text content to deserialize a json object, the deserialization will fail.

You can read more about the issue:

* [CloudBlockBlob.DownloadText() handles UTF8 BOM incorrectly](https://github.com/Azure/azure-storage-net/issues/46)
* [How to get rid of BOM when downloading text from azure blob](http://www.reza-aghaei.com/how-to-get-rid-of-bom-when-downloading-text-from-azure-blob/)

The behavior of `WebClient.DownloadString` is differnet. It handles `BOM` internally. I mixed some code from `WebClient` and `CloudBlockBlob` classes and created a `DownloadString` extension method for `CloudBlockBlob`, then to download text without BOM, you can simply use it this way:

    var connectionString = "connection string"
    var storageAccount = CloudStorageAccount.Parse(connectionString);
    var blobClient = storageAccount.CreateCloudBlobClient();
    var container = blobClient.GetContainerReference("container name");
    var blob = container.GetBlockBlobReference("blob name");
    var text = blob.DownloadString();
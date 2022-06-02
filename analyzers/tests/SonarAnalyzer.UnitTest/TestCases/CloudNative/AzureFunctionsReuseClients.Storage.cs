namespace FunctionApp1
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Specialized;
    using Azure.Storage.Files.DataLake;
    using Azure.Storage.Files.Shares;
    using Azure.Storage.Queues;
    using Microsoft.Azure.WebJobs;

    public static class Function1
    {
        [FunctionName("Sample")]
        public static void Run()
        {
            // The compliant clients usually pick up parameters from the request to pass to the constructor. They can not be made reusable.
            var blobService = new BlobServiceClient("connectionString");                    // Noncompliant
            var blobContainer = new BlobContainerClient("connectionString", "container");   // Compliant
            var blob = new BlobClient("connectionString", "container", "blob");             // Compliant
            var appendBlob = new AppendBlobClient("connectionString", "container", "blob"); // Compliant
            var blockBlob = new BlockBlobClient("connectionString", "container", "blob");   // Compliant
            var pageBlob = new PageBlobClient("connectionString", "container", "blob");     // Compliant

            var queueService = new QueueServiceClient("connectionString"); // Noncompliant
            var queue = new QueueClient("connectionString", "queueName");  // Compliant

            var shareService = new ShareServiceClient("connectionString");                                   // Noncompliant
            var share = new ShareClient("connectionString", "shareName");                                    // Compliant
            var shareDirectory = new ShareDirectoryClient("connectionString", "shareName", "directoryPath"); // Compliant
            var shareFile = new ShareFileClient("connectionString", "shareName", "filePath");                // Compliant

            var dataLakeService = new DataLakeServiceClient("connectionString");                                       // Noncompliant
            var dataLakeDirectory = new DataLakeDirectoryClient("connectionString", "fileSystemName", "direcoryPath"); // Compliant
            var dataLakeFile = new DataLakeFileClient("connectionString", "fileSystemName", "filePath");               // Compliant
            var dataLakePath = new DataLakePathClient("connectionString", "fileSystemName", "path");                   // Compliant
            var dataLakeFileSystem = new DataLakeFileSystemClient("connectionString", "fileSystemName");               // Compliant
        }
    }
}

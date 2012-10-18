using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DevelopingWithWindowsAzure.Shared.Storage
{
	public class StorageHelper
	{
		// JCTODO

		private const string DEFAULT_CONNECTION_STRING_NAME = "StorageConnectionString";

		public static CloudBlobClient GetClient(string connectionStringName)
		{
			// get a reference to the storage account
			var storageAccount = CloudStorageAccount.Parse(
				CloudConfigurationManager.GetSetting(connectionStringName));

			// return the blob client
			return storageAccount.CreateCloudBlobClient();
		}
		public static CloudBlobContainer GetContainer(string containerName)
		{
			return GetContainer(containerName, DEFAULT_CONNECTION_STRING_NAME);
		}
		public static CloudBlobContainer GetContainer(string containerName, string connectionStringName)
		{
			var client = GetClient(connectionStringName);

			// get a reference to the container
			var container =  client.GetContainerReference(containerName);

			// create the container if it doesn't exist
			container.CreateIfNotExist();

			// set the permissions on the container so that blobs are visible to the public
			container.SetPermissions(new BlobContainerPermissions()
			{
				PublicAccess = BlobContainerPublicAccessType.Blob
			});

			return container;
		}
		public static CloudBlob GetBlob(string containerName, string fileName)
		{
			return GetBlob(containerName, fileName, DEFAULT_CONNECTION_STRING_NAME);
		}
		public static CloudBlob GetBlob(string containerName, string fileName, string connectionStringName)
		{
			return GetContainer(containerName, connectionStringName).GetBlobReference(fileName);
		}
		public static CloudBlob GetNewBlob(string containerName, string fileName, out string newFileName)
		{
			return GetNewBlob(containerName, fileName, DEFAULT_CONNECTION_STRING_NAME, out newFileName);
		}
		public static CloudBlob GetNewBlob(string containerName, string fileName, 
			string connectionStringName, out string newFileName)
		{
			// JCTODO check to see if the blob exists before appending a date???
			var fileExtension = Path.GetExtension(fileName);
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			newFileName = string.Format("{0} {1:yyyyMMddhhmmss}{2}", 
				fileNameWithoutExtension, DateTime.Now, fileExtension);
			return GetBlob(containerName, newFileName, connectionStringName);
		}
	}
}

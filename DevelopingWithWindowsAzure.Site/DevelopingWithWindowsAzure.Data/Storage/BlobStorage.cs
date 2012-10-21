using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DevelopingWithWindowsAzure.Shared.Storage
{
	public class BlobStorage
	{
		private const string DEFAULT_CONNECTION_STRING_NAME = "StorageConnectionString";

		public class StorageItem
		{
			public string Name { get; set; }
			public Uri Uri { get; set; }
			public string AbsoluteUri
			{
				get
				{
					if (this.Uri != null)
						return this.Uri.AbsoluteUri;
					else
						return null;
				}
			}
		}

		public static CloudBlobClient GetClient(string connectionStringName)
		{
			// get a reference to the storage account
			var storageAccount = CloudStorageAccount.Parse(
				CloudConfigurationManager.GetSetting(connectionStringName));

			// return the blob client
			return storageAccount.CreateCloudBlobClient();
		}
		public static List<StorageItem> GetContainers()
		{
			return GetContainers(DEFAULT_CONNECTION_STRING_NAME);
		}
		public static List<StorageItem> GetContainers(string connectionStringName)
		{
			var listContainerItems = GetClient(connectionStringName).ListContainers();
			var containers = new List<StorageItem>();
			foreach (var listContainerItem in listContainerItems)
			{
				var container = new StorageItem();
				container.Uri = listContainerItem.Uri;
				container.Name = listContainerItem.Name;
				containers.Add(container);
			}
			return containers;
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
		public static void DeleteContainer(string containerName)
		{
			DeleteContainer(containerName, DEFAULT_CONNECTION_STRING_NAME);
		}
		public static void DeleteContainer(string containerName, string connectionStringName)
		{
			// delete each of the container's blobs
			var blobs = GetBlobs(containerName, connectionStringName);
			foreach (var blob in blobs)
				DeleteBlob(containerName, blob.Name);

			// delete the container
			var container = GetContainer(containerName, connectionStringName);
			container.Delete();
		}
		public static List<StorageItem> GetBlobs(string containerName)
		{
			return GetBlobs(containerName, DEFAULT_CONNECTION_STRING_NAME);
		}
		public static List<StorageItem> GetBlobs(string containerName, string connectionStringName)
		{
			BlobRequestOptions options = new BlobRequestOptions();
			options.BlobListingDetails = BlobListingDetails.All;
			options.UseFlatBlobListing = true;

			var listBlobItems = GetContainer(containerName, connectionStringName).ListBlobs(options);
			var blobs = new List<StorageItem>();
			foreach (var listBlobItem in listBlobItems)
			{
				var blob = new StorageItem();
				blob.Uri = listBlobItem.Uri;
				if (listBlobItem is CloudBlob)
					blob.Name = ((CloudBlob)listBlobItem).Name;
				blobs.Add(blob);
			}
			return blobs;
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
		public static void DeleteBlob(string containerName, string fileName)
		{
			DeleteBlob(containerName, fileName, DEFAULT_CONNECTION_STRING_NAME);
		}
		public static void DeleteBlob(string containerName, string fileName, string connectionStringName)
		{
			var blob = GetBlob(containerName, fileName, connectionStringName);
			try
			{
				blob.Delete();
			}
			catch (StorageClientException exc)
			{
				Trace.WriteLine(exc.Message);
			}
		}
	}
}

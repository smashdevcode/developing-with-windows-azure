using DevelopingWithWindowsAzure.Shared.Data;
using DevelopingWithWindowsAzure.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace DevelopingWithWindowsAzure.Shared.Helper
{
	public class VideoHelper
	{
		private IRepository _repository;

		public VideoHelper(IRepository repository)
		{
			_repository = repository;
		}

		public void SaveVideo(Video video)
		{
			// save the video to the database
			_repository.InsertOrUpdateVideo(video);

			// JCTODO move to storage helper class

			// get a reference to the storage account
			var storageAccount = CloudStorageAccount.Parse(
				CloudConfigurationManager.GetSetting("StorageConnectionString"));

			// create the blob client
			var blobClient = storageAccount.CreateCloudBlobClient();

			// attempt to get a reference to the container
			// and if it doesn't exist, create it
			var container = blobClient.GetContainerReference("videos");
			container.CreateIfNotExist();

			// set the permissions on the container so that blobs are visible to the public
			container.SetPermissions(new BlobContainerPermissions() 
			{
				PublicAccess = BlobContainerPublicAccessType.Blob
			});

			// retrieve reference to the blob
			var blob = container.GetBlobReference(video.FileName);

			// create the blob
			blob.UploadFromStream(video.FileData);
			//using (var memoryStream = new System.IO.MemoryStream(video.FileData))
			//{
			//	blob.UploadFromStream(memoryStream);
			//}
		}
	}
}

using DevelopingWithWindowsAzure.Shared.Data;
using DevelopingWithWindowsAzure.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using DevelopingWithWindowsAzure.Shared.Queue;
using Microsoft.ServiceBus.Messaging;
using DevelopingWithWindowsAzure.Shared.Storage;

namespace DevelopingWithWindowsAzure.Shared.Media
{
	public class VideoProcessor
	{
		public const string VIDEOS_CONTAINER = "videos";

		private IRepository _repository;

		public VideoProcessor(IRepository repository)
		{
			_repository = repository;
		}

		public void SaveVideo(Video video)
		{
			// retrieve reference to the blob
			string newFileName = null;
			var blob = BlobStorage.GetNewBlob(VIDEOS_CONTAINER, video.FileName, out newFileName);

			// update the file name if it's been changed
			if (video.FileName != newFileName)
				video.FileName = newFileName;

			// JCTODO move to another helper method???
			// create the blob
			blob.UploadFromStream(video.FileData);
			//using (var memoryStream = new System.IO.MemoryStream(video.FileData))
			//{
			//	blob.UploadFromStream(memoryStream);
			//}

			// save the video to the database
			_repository.InsertOrUpdateVideo(video);

			// send the message
			var client = QueueConnector.GetQueueClient();
			// JCTODO move to a method on the QueueConnector class???
			client.Send(new BrokeredMessage(video.VideoID));
		}
	}
}

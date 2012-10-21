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
			// upload the file
			var newFileName = BlobStorage.UploadBlob(VIDEOS_CONTAINER, video.FileName, video.FileData);

			// update the file name if it's been changed
			if (video.FileName != newFileName)
				video.FileName = newFileName;

			// save the video to the database
			_repository.InsertOrUpdateVideo(video);

			// send the message to the video processor
			QueueConnector.SendMessage(video.VideoID);
		}
		public void DeleteVideo(int videoID)
		{
			// get the video
			var video = _repository.GetVideo(videoID);

			// delete the blob
			BlobStorage.DeleteBlob(VIDEOS_CONTAINER, video.FileName);

			// delete the video from the database
			_repository.DeleteVideo(video);
		}
	}
}

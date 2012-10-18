using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using DevelopingWithWindowsAzure.Shared.Queue;
using DevelopingWithWindowsAzure.Shared.Data;
using Microsoft.WindowsAzure.MediaServices.Client;
using DevelopingWithWindowsAzure.Shared.Entities;
using System.IO;
using DevelopingWithWindowsAzure.Shared.Storage;
using DevelopingWithWindowsAzure.Shared.Helper;
using DevelopingWithWindowsAzure.Shared.Enums;

namespace WorkerRoleWithSBQueue1
{
	public class WorkerRole : RoleEntryPoint
	{
		private QueueClient _queueClient = null;
		private bool _isStopped = true;

		public override void Run()
		{
			while (!_isStopped)
			{
				try
				{
					// Receive the message
					BrokeredMessage receivedMessage = null;
					receivedMessage = _queueClient.Receive();

					if (receivedMessage != null)
					{
						// JCTODO remove
						//receivedMessage.DeadLetter();



						Trace.WriteLine("Processing", receivedMessage.SequenceNumber.ToString());

						// get the video
						var repository = new Repository();
						var videoID = receivedMessage.GetBody<int>();
						var video = repository.GetVideo(videoID);

						if (video != null)
						{
							switch (video.VideoStatusEnum)
							{
								case VideoStatus.Uploaded:
									if (video.VideoStatusEnum != VideoStatus.Processing)
									{
										video.VideoStatusEnum = VideoStatus.Processing;
										repository.InsertOrUpdateVideo(video);
									}

									// JCTODO update method to throw specific exceptions???
									// if the asset or source file is not found, then update the message to be a dead letter
									CreateEncodingJob(video);

									video.VideoStatusEnum = VideoStatus.Processed;
									repository.InsertOrUpdateVideo(video);

									receivedMessage.Complete();
									break;
								case VideoStatus.Processing:
									// JCTODO is this the right thing to do???
									// would it be better to get the asset and/or job and check on status???
									receivedMessage.Abandon();
									break;
								case VideoStatus.Processed:
									// mark the message as complete
									receivedMessage.Complete();
									break;
								default:
									throw new ApplicationException("Unexpected VideoStatus enum value: " + video.VideoStatusEnum.ToString());
							}
						}
						else
							receivedMessage.Abandon();
					}
				}
				catch (MessagingException e)
				{
					if (!e.IsTransient)
					{
						Trace.WriteLine(e.Message);
						throw;
					}

					Thread.Sleep(10000);
				}
				catch (OperationCanceledException e)
				{
					if (!_isStopped)
					{
						Trace.WriteLine(e.Message);
						throw;
					}
				}
			}
		}
		public override bool OnStart()
		{
			// set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			_queueClient = QueueConnector.GetQueueClient();
			_isStopped = false;
			return base.OnStart();
		}
		public override void OnStop()
		{
			_isStopped = true;
			_queueClient.Close();
			base.OnStop();
		}

		// JCTODO move to configuration file
		private const string MEDIA_SERVICES_ACCOUNT_NAME = "developingwithazure";
		private const string MEDIA_SERVICES_ACCOUNT_KEY = "EqDLzcDnFGDGUXxqZ5M9PBvT2r+pT8Rf9RKqQvyXDUc=";

		private static CloudMediaContext _mediaContext = null;

		private static CloudMediaContext GetMediaContext()
		{
			if (_mediaContext == null)
				_mediaContext = new CloudMediaContext(MEDIA_SERVICES_ACCOUNT_NAME, MEDIA_SERVICES_ACCOUNT_KEY);
			return _mediaContext;
		}
		private static IMediaProcessor GetMediaProcessor(string mediaProcessorName)
		{
			var mediaContext = GetMediaContext();

			// query for a media processor to get a reference
			var mediaProcessor = (from p in mediaContext.MediaProcessors
								  where p.Name == mediaProcessorName
								  select p).FirstOrDefault();
			if (mediaProcessor == null)
			{
				throw new ArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture,
					"Unknown processor", mediaProcessorName));
			}

			return mediaProcessor;
		}
		private static void CreateEncodingJob(Video video)
		{
			var mediaContext = GetMediaContext();




			// create an empty asset
			IAsset asset = mediaContext.Assets.CreateEmptyAsset(
				string.Format("Asset_VideoID_{0}", video.VideoID), AssetCreationOptions.None);

			// create a locator to get the SAS URL
			IAccessPolicy writePolicy = mediaContext.AccessPolicies.Create("Policy For Copying", TimeSpan.FromMinutes(30), AccessPermissions.Write | AccessPermissions.List);
			ILocator destinationLocator = mediaContext.Locators.CreateSasLocator(asset, writePolicy, DateTime.UtcNow.AddMinutes(-5));

			////Create CloudBlobClient:
			//var storageInfo = new StorageCredentialsAccountAndKey("YourStorageAccount", "YourStoragePassword");
			//CloudBlobClient cloudClient = new CloudBlobClient("http://YourStorageAccount.blob.core.windows.net", storageInfo);

			//// get a blob client
			//var blobClient = StorageHelper.GetClient();
	
			// create the reference to the destination container
			var destinationFileUrl = new Uri(destinationLocator.Path);
			var destinationContainerName = destinationFileUrl.Segments[1];
			//var destinationContainer = StorageHelper.GetContainer(destinationContainerName);

			//// create the reference to the source container
			//var sourceContainer = StorageHelper.GetContainer(VideoHelper.VIDEOS_CONTAINER);

			// get and validate the source blob, in this case a file called FileToCopy.mp4:
			//CloudBlob sourceFileBlob = sourceContainer.GetBlobReference("FileToCopy.mp4");
			var sourceFileBlob = StorageHelper.GetBlob(VideoHelper.VIDEOS_CONTAINER, video.FileName);
			sourceFileBlob.FetchAttributes();
			var sourceLength = sourceFileBlob.Properties.Length;
			System.Diagnostics.Debug.Assert(sourceLength > 0);

			// if we got here then we can assume the source is valid and accessible

			// create destination blob for copy, in this case, we choose to rename the file
			//CloudBlob destinationFileBlob = destinationContainer.GetBlobReference("CopiedFile.mp4");
			var destinationFileBlob = StorageHelper.GetBlob(destinationContainerName, video.FileName);
			destinationFileBlob.CopyFromBlob(sourceFileBlob);  // will fail here if project references are bad (the are lazy loaded)

			// check destination blob
			destinationFileBlob.FetchAttributes();
			System.Diagnostics.Debug.Assert(sourceFileBlob.Properties.Length == sourceLength);

			// if we got here then the copy worked

			// publish the asset
			asset.Publish();
			asset = RefreshAsset(asset);





			//// create an unencrypted asset and upload to storage
			//IAsset asset = mediaContext.Assets.Create(inputMediaFilePath, AssetCreationOptions.None);

			// declare a new job
			IJob job = mediaContext.Jobs.Create(string.Format("EncodingJob_VideoID_{0}", video.VideoID));

			// get a media processor reference, and pass to it the name of the 
			// processor to use for the specific task
			IMediaProcessor processor = GetMediaProcessor("Windows Azure Media Encoder");

			// create a task with the encoding details, using a string preset
			ITask task = job.Tasks.AddNew(
				string.Format("EncodingTask_H264_VideoID_{0}", video.VideoID),
				processor,
				"H.264 256k DSL CBR",
				TaskCreationOptions.None);

			// Specify the input asset to be encoded
			task.InputMediaAssets.Add(asset);

			// add an output asset to contain the results of the job
			task.OutputMediaAssets.AddNew(string.Format("{0} H264", video.FileName), 
				true, AssetCreationOptions.None);

			// launch the job
			job.Submit();

			// checks job progress
			CheckJobProgress(job.Id);

			// get an updated job reference
			// after waiting for the job on the thread in the CheckJobProgress method
			job = GetJob(job.Id);

			// get a reference to the output asset from the job
			IAsset outputAsset = job.OutputMediaAssets[0];

			// set the class-level variable so you can retrieve the asset later
			//_outputAssetID = outputAsset.Id;

			// you can optionally get a SAS URL to the output asset
			//string sasUrl = GetAssetSasUrl(outputAsset, TimeSpan.FromMinutes(30));

			// write the URL to a local file
			// you can use the saved SAS URL to browse directly to the asset
			//string outFilePath = Path.GetFullPath(outputFolder + @"\" + "SasUrl.txt");
			//WriteToFile(outFilePath, sasUrl);		
		}
		private static IAsset RefreshAsset(IAsset asset)
		{
			var mediaContext = GetMediaContext();

			return (from a in mediaContext.Assets
					where a.Id == asset.Id
					select a).FirstOrDefault();
		}
		private static void CheckJobProgress(string jobID)
		{
			// flag to indicate when job state is finished
			bool jobCompleted = false;
			// expected polling interval in milliseconds
			// adjust this interval as needed based on estimated job completion times
			const int jobProgressInterval = 20000;

			while (!jobCompleted)
			{
				// get an updated reference to the job in case 
				// reference gets 'stale' while thread waits
				IJob theJob = GetJob(jobID);

				// check job and report state
				switch (theJob.State)
				{
					case JobState.Finished:
						jobCompleted = true;
						Trace.WriteLine("Job finished...");
						break;
					case JobState.Queued:
					case JobState.Scheduled:
					case JobState.Processing:
						Trace.WriteLine("Job state: " + theJob.State);
						Trace.WriteLine("Please wait...");
						break;
					case JobState.Error:
						throw new ApplicationException("Encoding task failed.");
					default:
						Trace.WriteLine(theJob.State.ToString());
						break;
				}

				// wait for the specified job interval before checking state again
				Thread.Sleep(jobProgressInterval);
			}
		}
		private static IJob GetJob(string jobID)
		{
			var mediaContext = GetMediaContext();

			var job = (from j in mediaContext.Jobs
					  where j.Id == jobID
					  select j).FirstOrDefault();

			// confirm whether job exists, and return
			if (job != null)
				return job;
			else
				Trace.WriteLine("Job does not exist.");
			return null;
		}
		//private static String GetAssetSasUrl(IAsset asset, TimeSpan accessPolicyTimeout)
		//{
		//	// Create a policy for the asset.
		//	IAccessPolicy readPolicy = _context.AccessPolicies.Create("My Test Policy", accessPolicyTimeout, AccessPermissions.Read);

		//	// Create a locator for the asset. This assigns the policy to the asset and returns a locator. 
		//	// Also specify the startTime parameter, setting it 5 minutes before "Now", so that the locator
		//	// is accessible right away even if there is clock skew between the server time and local time. 
		//	ILocator locator = _context.Locators.CreateSasLocator(asset,
		//		readPolicy,
		//		DateTime.UtcNow.AddMinutes(-5));

		//	// Print the path for the locator you created.
		//	Console.WriteLine("Locator path: ");
		//	Console.WriteLine(locator.Path);
		//	Console.WriteLine();

		//	// Get the asset file name, you use this to create the SAS URL. In this sample, 
		//	// get an output file with an .mp4 ending. We know that .mp4 is the type of 
		//	// output file the encoding job produces and we don't want a link to the 
		//	// .xml metadata file on the server, in this case.
		//	var theOutputFile =
		//						from f in asset.Files
		//						where f.Name.EndsWith(".mp4")
		//						select f;
		//	// Cast the IQueryable variable back to an IFileInfo.
		//	IFileInfo theFile = theOutputFile.FirstOrDefault();
		//	string fileName = theFile.Name;

		//	// Now take the locator path, add the file name, and build a complete SAS URL to browse to the asset.
		//	var uriBuilder = new UriBuilder(locator.Path);
		//	uriBuilder.Path += "/" + fileName;

		//	// Print the full SAS URL
		//	Console.WriteLine("Full URL to file: ");
		//	Console.WriteLine(uriBuilder.Uri.AbsoluteUri);
		//	Console.WriteLine();

		//	// Return the SAS URL. 
		//	return uriBuilder.Uri.AbsoluteUri;
		//}
		//private static void WriteToFile(string outFilePath, string fileContent)
		//{
		//	StreamWriter sr = File.CreateText(outFilePath);
		//	sr.Write(fileContent);
		//	sr.Close();
		//}
	}
}

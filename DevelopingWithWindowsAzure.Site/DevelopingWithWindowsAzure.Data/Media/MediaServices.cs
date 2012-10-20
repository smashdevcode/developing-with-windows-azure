using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.MediaServices.Client;
using DevelopingWithWindowsAzure.Shared.Entities;
using System.Diagnostics;
using DevelopingWithWindowsAzure.Shared.Storage;

namespace DevelopingWithWindowsAzure.Shared.Media
{
	public class MediaServices : IDisposable
	{
		// JCTODO move to configuration file
		private const string MEDIA_SERVICES_ACCOUNT_NAME = "developingwithazure";
		private const string MEDIA_SERVICES_ACCOUNT_KEY = "EqDLzcDnFGDGUXxqZ5M9PBvT2r+pT8Rf9RKqQvyXDUc=";

		private CloudMediaContext _context = null;

		public MediaServices()
		{
			_context = new CloudMediaContext(MEDIA_SERVICES_ACCOUNT_NAME, MEDIA_SERVICES_ACCOUNT_KEY);
		}

		#region Context Collections
		public List<IAsset> GetAssets()
		{
			return _context.Assets.ToList();
		}
		public List<IContentKey> GetContentKeys()
		{
			return _context.ContentKeys.ToList();
		}
		public List<IFileInfo> GetFiles()
		{
			return _context.Files.ToList();
		}
		public List<IJob> GetJobs()
		{
			return _context.Jobs.ToList();
		}
		public List<ILocator> GetLocators()
		{
			return _context.Locators.ToList();
		}
		public List<IMediaProcessor> GetMediaProcessors()
		{
			return _context.MediaProcessors.ToList();
		}
		#endregion
		#region Assets
		private IAsset GetAsset(string assetID)
		{
			var asset = (from a in _context.Assets
						 where a.Id == assetID
						 select a).FirstOrDefault();
			// confirm whether asset exists, and return
			if (asset != null)
				return asset;
			else
				Trace.WriteLine(string.Format("Asset does not exist: {0}", assetID));
			return null;
		}
		public void DeleteAsset(string assetID)
		{
			var asset = GetAsset(assetID);
			if (asset != null)
			{
				var locators = _context.Locators.Where(l => l.AssetId == assetID);
				foreach (var l in locators)
					_context.Locators.Revoke(l);

				//var numberOfContentKeys = asset.ContentKeys.Count();
				//for (int i = 0; i < numberOfContentKeys; i++)
				//	asset.ContentKeys.RemoveAt(i);
				foreach (var contentKey in asset.ContentKeys)
					_context.ContentKeys.Delete(contentKey);

				_context.Assets.Delete(asset);
			}
		}
		#endregion
		public void CreateEncodingJob(Video video)
		{
			// JCTODO put into a method to get a new asset for a video???

			// create an empty asset
			IAsset asset = _context.Assets.CreateEmptyAsset(
				string.Format("Asset_VideoID_{0}", video.VideoID), AssetCreationOptions.None);

			// create a locator to get the SAS URL
			IAccessPolicy writePolicy = _context.AccessPolicies.Create("Policy For Copying", TimeSpan.FromMinutes(30), AccessPermissions.Write | AccessPermissions.List);
			ILocator destinationLocator = _context.Locators.CreateSasLocator(asset, writePolicy, DateTime.UtcNow.AddMinutes(-5));

			// create the reference to the destination container
			var destinationFileUrl = new Uri(destinationLocator.Path);
			var destinationContainerName = destinationFileUrl.Segments[1];

			// get and validate the source blob, in this case a file called FileToCopy.mp4:
			var sourceFileBlob = BlobStorage.GetBlob(VideoProcessor.VIDEOS_CONTAINER, video.FileName);
			sourceFileBlob.FetchAttributes();
			var sourceLength = sourceFileBlob.Properties.Length;
			Debug.Assert(sourceLength > 0);

			// if we got here then we can assume the source is valid and accessible

			// create destination blob for copy, in this case, we choose to rename the file
			var destinationFileBlob = BlobStorage.GetBlob(destinationContainerName, video.FileName);
			destinationFileBlob.CopyFromBlob(sourceFileBlob);  // will fail here if project references are bad (the are lazy loaded)

			// check destination blob
			destinationFileBlob.FetchAttributes();
			System.Diagnostics.Debug.Assert(sourceFileBlob.Properties.Length == sourceLength);

			// if we got here then the copy worked

			// publish the asset
			asset.Publish();

			// refresh the asset
			asset = GetAsset(asset.Id);




			// declare a new job
			var job = _context.Jobs.Create(string.Format("EncodingJob_VideoID_{0}", video.VideoID));

			// get a media processor reference, and pass to it the name of the 
			// processor to use for the specific task
			var processor = GetMediaProcessor("Windows Azure Media Encoder");

			// create a task with the encoding details, using a string preset
			var task = job.Tasks.AddNew(
				string.Format("EncodingTask_H264_VideoID_{0}", video.VideoID),
				processor,
				"H.264 256k DSL CBR",
				TaskCreationOptions.None);

			// Specify the input asset to be encoded
			task.InputMediaAssets.Add(asset);

			// add an output asset to contain the results of the job
			task.OutputMediaAssets.AddNew(string.Format("{0} H264", video.FileName),
				true, AssetCreationOptions.None);

			// submit the job
			job.Submit();

			// checks job progress
			//CheckJobProgress(job.Id);

			// get an updated job reference
			// after waiting for the job on the thread in the CheckJobProgress method
			//job = GetJob(job.Id);

			// get a reference to the output asset from the job
			//IAsset outputAsset = job.OutputMediaAssets[0];

			// set the class-level variable so you can retrieve the asset later
			//_outputAssetID = outputAsset.Id;

			// you can optionally get a SAS URL to the output asset
			//string sasUrl = GetAssetSasUrl(outputAsset, TimeSpan.FromMinutes(30));

			// write the URL to a local file
			// you can use the saved SAS URL to browse directly to the asset
			//string outFilePath = Path.GetFullPath(outputFolder + @"\" + "SasUrl.txt");
			//WriteToFile(outFilePath, sasUrl);		
		}

		private IMediaProcessor GetMediaProcessor(string mediaProcessorName)
		{
			// query for a media processor to get a reference
			var mediaProcessor = (from p in _context.MediaProcessors
								  where p.Name == mediaProcessorName
								  select p).FirstOrDefault();
			if (mediaProcessor == null)
				throw new ArgumentException(string.Format("Unknown processor: {0}", mediaProcessorName));
			return mediaProcessor;
		}
		private IJob GetJob(string jobID)
		{
			var job = (from j in _context.Jobs
					   where j.Id == jobID
					   select j).FirstOrDefault();
			// confirm whether job exists, and return
			if (job != null)
				return job;
			else
				Trace.WriteLine(string.Format("Job does not exist: {0}", jobID));
			return null;
		}
		// JCTODO setup this method!!!
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
		//private void CheckJobProgress(string jobID)
		//{
		//	// flag to indicate when job state is finished
		//	bool jobCompleted = false;
		//	// expected polling interval in milliseconds
		//	// adjust this interval as needed based on estimated job completion times
		//	const int jobProgressInterval = 20000;

		//	while (!jobCompleted)
		//	{
		//		// get an updated reference to the job in case 
		//		// reference gets 'stale' while thread waits
		//		IJob theJob = GetJob(jobID);

		//		// check job and report state
		//		switch (theJob.State)
		//		{
		//			case JobState.Finished:
		//				jobCompleted = true;
		//				Trace.WriteLine("Job finished...");
		//				break;
		//			case JobState.Queued:
		//			case JobState.Scheduled:
		//			case JobState.Processing:
		//				Trace.WriteLine("Job state: " + theJob.State);
		//				Trace.WriteLine("Please wait...");
		//				break;
		//			case JobState.Error:
		//				throw new ApplicationException("Encoding task failed.");
		//			default:
		//				Trace.WriteLine(theJob.State.ToString());
		//				break;
		//		}

		//		// wait for the specified job interval before checking state again
		//		Thread.Sleep(jobProgressInterval);
		//	}
		//}

		public void Dispose()
		{
			_context.DetachAll();
		}
	}
}

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

namespace WorkerRoleWithSBQueue1
{
	public class WorkerRole : RoleEntryPoint
	{
		private QueueClient _client = null;
		private bool _isStopped = true;

		public override void Run()
		{
			while (!_isStopped)
			{
				try
				{
					// Receive the message
					BrokeredMessage receivedMessage = null;
					receivedMessage = _client.Receive();

					if (receivedMessage != null)
					{
						Trace.WriteLine("Processing", receivedMessage.SequenceNumber.ToString());

						// get the video
						var repository = new Repository();
						var videoID = receivedMessage.GetBody<int>();
						var video = repository.GetVideo(videoID);

						if (video != null)
						{
							video.VideoStatusEnum = DevelopingWithWindowsAzure.Shared.Enums.VideoStatus.Processed;
							repository.InsertOrUpdateVideo(video);
						}

						receivedMessage.Complete();
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
			// Set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			_client = QueueConnector.GetQueueClient();
			_isStopped = false;
			return base.OnStart();
		}
		public override void OnStop()
		{
			_isStopped = true;
			_client.Close();
			base.OnStop();
		}
	}
}

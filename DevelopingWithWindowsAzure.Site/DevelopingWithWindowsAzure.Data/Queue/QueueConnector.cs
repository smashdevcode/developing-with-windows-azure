using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopingWithWindowsAzure.Shared.Queue
{
	// JCTODO rename as VideosToProcessQueue???
	// JCTODO setup QueueConnector base class???
	public static class QueueConnector
	{
		public static QueueClient VideosToProcessQueueClient;

		// JCTODO move to config file???
		//public const string Namespace = "developingwithazure";
		//public const string IssuerName = "owner";
		//public const string IssuerKey = "z9214eUotU8zTjO79FMAS+myrVuYwVA/PMOjiPHqV7M=";

		private const string SERVICE_BUS_QUEUE_NAME = "VideosToProcess";

		public static QueueClient GetQueueClient()
		{
			Initialize();
			return VideosToProcessQueueClient;
		}
		public static NamespaceManager CreateNamespaceManager()
		{
			//var uri = ServiceBusEnvironment.CreateServiceUri(
			//	"sb", Namespace, string.Empty);
			//var tp = TokenProvider.CreateSharedSecretTokenProvider(
			//	IssuerName, IssuerKey);
			//return new NamespaceManager(uri, tp);

			// get the service bus connection string
			var serviceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

			// get the namespace manager
			var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);

			return namespaceManager;
		}
		public static void Initialize()
		{
			if (VideosToProcessQueueClient != null)
				return;

			// JCTODO is this necessary to set??? okay to let it autodetect the ports???
			//ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;

			// create the queue if it doesn't exist
			var namespaceManager = CreateNamespaceManager();
			if (!namespaceManager.QueueExists(SERVICE_BUS_QUEUE_NAME))
				namespaceManager.CreateQueue(SERVICE_BUS_QUEUE_NAME);

			// initialize the queue client
			var messagingFactory = MessagingFactory.Create(
				namespaceManager.Address,
				namespaceManager.Settings.TokenProvider);
			VideosToProcessQueueClient = messagingFactory.CreateQueueClient(SERVICE_BUS_QUEUE_NAME);
		}
	}
}

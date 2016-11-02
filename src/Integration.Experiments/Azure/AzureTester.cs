using System;
using System.IO;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Azure;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Azure.Infrastructure.ServiceBus;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Experiments.Azure
{
	public static class AzureTester
	{
		public static ApplicationConfiguration TestAzure(this ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			application
				.Tasks(tasks => tasks
					.Task<AzureServiceBusTesterTask>()
					.Task<AzureBlobStorageTesterTask>())
				.Hosts(hosts => hosts
					.Host<AzureServiceBusHost>())
				.UseAzure(azure => azure
					.ServiceBus(serviceBus => serviceBus
						.DefaultConnection(ConnectionString.FromName("AzureServiceBus")))
					.BlobStorage(blobStorage => blobStorage
						.ReplaceArchiver(ConnectionString.FromName("AzureBlobStorage"))
						.DefaultConnection(ConnectionString.FromName("AzureBlobStorage"))));

			return application;
		}
	}

	public class AzureBlobStorageTesterTask : Task
	{
		private readonly IAzureBlobStorageClientFactory _blobStorageClientFactory;

		public AzureBlobStorageTesterTask(IAzureBlobStorageClientFactory blobStorageClientFactory)
		{
			_blobStorageClientFactory = blobStorageClientFactory;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			CloudBlobClient client = _blobStorageClientFactory.CreateBlobClient();

			CloudBlobContainer container = client.GetContainerReference("task");
			container.CreateIfNotExists();

			string id = Guid.NewGuid().ToString("N");

			CloudBlockBlob newBlock = container.GetBlockBlobReference(id);
			newBlock.UploadFromFile(@"D:\Dropbox\Photos\ipad_IMG_6578.jpg");

			CloudBlockBlob savedBlock = container.GetBlockBlobReference(id);

			using (var memoryStream = new MemoryStream())
			{
				savedBlock.DownloadToStream(memoryStream);

				File.WriteAllBytes(@"c:\tmp\test.jpg", memoryStream.ToArray());
			}
		}

		public override string Description => "TBD";
	}

	public class AzureServiceBusTesterTask : Task
	{
		private readonly IAzureServiceBusClientFactory _serviceBusClientFactory;

		public AzureServiceBusTesterTask(IAzureServiceBusClientFactory serviceBusClientFactory)
		{
			_serviceBusClientFactory = serviceBusClientFactory;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			NamespaceManager manager = _serviceBusClientFactory.CreateNamespaceManager();

			if (!manager.QueueExists("TestQueue"))
				manager.CreateQueue("TestQueue");

			QueueClient client = _serviceBusClientFactory.CreateQueueClient("TestQueue");

			client.Send(new BrokeredMessage("Hello") { ReplyTo = "MyQueue" });
			client.Send(new BrokeredMessage(new[] { "Test", "Test2", "Test3"}) { ReplyTo = "MyQueue" });
		}

		public override string Description => "TBD";
	}

	public class AzureServiceBusHost : IHost
	{
		private readonly IAzureServiceBusClientFactory _serviceBusClientFactory;
		private readonly IConsoleWriter _console;

		public AzureServiceBusHost(IAzureServiceBusClientFactory serviceBusClientFactory, IConsoleWriter console)
		{
			_serviceBusClientFactory = serviceBusClientFactory;
			_console = console;
		}

		public bool CanHandle(HostArguments args)
		{
			return string.Equals(this.Name(), args.Command, StringComparison.OrdinalIgnoreCase);
		}

		public void Handle(HostArguments args)
		{
			bool running = true;

			string queueName = "ChatQueue";

			var manager = _serviceBusClientFactory.CreateNamespaceManager();

			if (!manager.QueueExists(queueName))
				manager.CreateQueue(queueName);

			System.Threading.Tasks.Task sender = System.Threading.Tasks.Task.Run(() =>
			{
				QueueClient queueClient = _serviceBusClientFactory.CreateQueueClient(queueName);

				_console.RepeatUntilEscapeKeyIsHit(() =>
				{
					_console.WriteLine("Message: ");

					queueClient.Send(new BrokeredMessage(Console.ReadLine()));
				});

				running = false;
			});

			var receiver = System.Threading.Tasks.Task.Run(() =>
			{
				QueueClient queueClient = _serviceBusClientFactory.CreateQueueClient(queueName);

				while (running)
				{
					try
					{
						BrokeredMessage message = queueClient.Receive(TimeSpan.FromSeconds(2));

						if (message != null)
						{
							using (message)
							{
								_console.WriteLine(message.GetBody<string>());

								message.Complete();
							}							
						}
					}
					catch (Exception ex)
					{
						_console.WriteLine(ex.Message);
					}
				}
			});

			System.Threading.Tasks.Task.WaitAll(sender, receiver);
		}

		public string Description => "TBD";
	}
}
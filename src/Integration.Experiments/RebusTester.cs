using System;
using System.IO;
using Microsoft.Azure;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Routing.TypeBased;
using Vertica.Integration.Model;
using Vertica.Integration.Rebus;
using Task = System.Threading.Tasks.Task;

namespace Vertica.Integration.Experiments
{
	public static class RebusTester
	{
		public static ApplicationConfiguration TestRebus(this ApplicationConfiguration application)
		{
			string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

			return application
				.UseRebus(rebus => rebus
					.Bus(bus => bus
						.Routing(routing => routing.TypeBased().Map<string>("InputQueue_July30rd"))
						.Transport(transport => transport.UseAzureServiceBus(connectionString, "InputQueue_July30rd")))
					.Handlers(handlers => handlers
						.Handler<ChatHandler>()
						.AddFromAssemblyOfThis<ChatHandler>()))
				.Tasks(tasks => tasks
					.Task<RebusTask>()
					.Task<OtherRebusTask>()
					.Task<NoRebusTask>());
		}
	}

	public class ChatHandler : IHandleMessages<string>
	{
		private readonly TextWriter _outputter;

		public ChatHandler(TextWriter outputter)
		{
			_outputter = outputter;
		}

		public Task Handle(string message)
		{
			return _outputter.WriteLineAsync(message);
		}
	}

	public class RebusTask : Model.Task
	{
		private readonly Lazy<IBus> _bus;
		private readonly ITaskRunner _taskRunner;
		private readonly ITaskFactory _taskFactory;

		public RebusTask(Lazy<IBus> bus, ITaskRunner taskRunner, ITaskFactory taskFactory)
		{
			_bus = bus;
			_taskRunner = taskRunner;
			_taskFactory = taskFactory;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			_bus.Value.Send("Hello from Task!");

			_taskRunner.Execute(_taskFactory.Get<OtherRebusTask>());
		}

		public override string Description
		{
			get { return "Writes messages to Rebus"; }
		}
	}

	public class OtherRebusTask : Model.Task
	{
		private readonly Func<IBus> _bus;

		public OtherRebusTask(Func<IBus> bus)
		{
			_bus = bus;
		}

		public override string Description
		{
			get { return "Hello"; }
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			_bus().Send("Hello from other task.");
		}
	}

	public class NoRebusTask : Model.Task
	{
		public override string Description
		{
			get { return "Hello"; }
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			context.Log.Message("No Rebus!");
		}
	}
}
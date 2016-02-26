using System;
using System.IO;
using System.Linq;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Routing.TypeBased;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Rebus;
using Task = System.Threading.Tasks.Task;

namespace Vertica.Integration.Experiments.Rebus
{
	public static class RebusTester
	{
		public static ApplicationConfiguration TestRebus(this ApplicationConfiguration application, string[] args)
		{
			string connectionString = ConnectionString.FromName("AzureServiceBus");

			return application
				.UseRebus(rebus => rebus
					.Bus(bus => bus
						.Routing(routing => routing.TypeBased().Map<string>("InputQueue_RebusHost"))
						.Transport(transport => transport.UseAzureServiceBus(connectionString, "InputQueue_" + args[0])))
					.Handlers(handlers => handlers
						.AddFromAssemblyOfThis<StringHandler>()))
				.Tasks(tasks => tasks
					.Task<RebusTask>());
		}
	}

	public class StringHandler : IHandleMessages<string>
	{
		private readonly TextWriter _outputter;
		private readonly IBus _bus;

		public StringHandler(TextWriter outputter, IBus bus)
		{
			_outputter = outputter;
			_bus = bus;
		}

		public Task Handle(string message)
		{
			return _outputter.WriteLineAsync(message)
				.ContinueWith(t => _bus.Reply(message.ToCharArray().Select(x => x.ToString()).ToArray()));
		}
	}

	public class StringArrayHandler : IHandleMessages<string[]>
	{
		private readonly TextWriter _outputter;

		public StringArrayHandler(TextWriter outputter)
		{
			_outputter = outputter;
		}

		public Task Handle(string[] message)
		{
			return _outputter.WriteLineAsync(String.Join(", ", message));
		}
	}

	public class RebusTask : Model.Task
	{
		private readonly Func<IBus> _bus;
		private readonly TextWriter _writer;

		public RebusTask(Func<IBus> bus, TextWriter writer)
		{
			_bus = bus;
			_writer = writer;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			_writer.RepeatUntilEscapeKeyIsHit(() =>
			{
				_bus().Send(Console.ReadLine()).Wait();
			});
		}

		public override string Description
		{
			get { return "Writes messages to Rebus"; }
		}
	}
}
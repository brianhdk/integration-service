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
		public static ApplicationConfiguration TestRebus(this ApplicationConfiguration application, string[] args)
		{
			string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

			return application
				.UseRebus(rebus => rebus
					.Bus(bus => bus
						.Routing(routing => routing.TypeBased()
							.Map<string>("InputQueue_RebusHost")
							.Map<DateTimeOffset>("InputQueue_RebusTask")
						)
						.Transport(transport => transport.UseAzureServiceBus(connectionString, "InputQueue_" + args[0])))
					.Handlers(handlers => handlers
						.AddFromAssemblyOfThis<ChatHandler>()))
				.Tasks(tasks => tasks
					.Task<RebusTask>());
		}
	}

	public class ChatHandler : IHandleMessages<string>
	{
		private readonly TextWriter _outputter;
		private readonly IBus _bus;

		public ChatHandler(TextWriter outputter, IBus bus)
		{
			_outputter = outputter;
			_bus = bus;
		}

		public Task Handle(string message)
		{
			return _outputter.WriteLineAsync(message)
				.ContinueWith(t => _bus.Reply(new[] { "a", "b", "c"}));
		}
	}

	public class DateHandler : IHandleMessages<string[]>
	{
		private readonly TextWriter _outputter;

		public DateHandler(TextWriter outputter)
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

		public RebusTask(Func<IBus> bus)
		{
			_bus = bus;
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			do
			{
				_bus().Send(Console.ReadLine()).Wait();

				Console.WriteLine(@"Press ESCAPE to stop Rebus...");
				Console.WriteLine();

			} while (WaitingForEscape());
		}

		private static bool WaitingForEscape()
		{
			return Console.ReadKey(intercept: true /* don't display */).Key != ConsoleKey.Escape;
		}

		public override string Description
		{
			get { return "Writes messages to Rebus"; }
		}
	}
}
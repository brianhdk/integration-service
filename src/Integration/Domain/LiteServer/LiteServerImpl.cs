using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Domain.LiteServer
{
	internal class LiteServerImpl : IDisposable
	{
	    private readonly IKernel _kernel;
	    private readonly IConsoleWriter _console;
	    private readonly InternalConfiguration _configuration;

		private readonly HouseKeeping _houseKeeping;

		public LiteServerImpl(IKernel kernel, InternalConfiguration configuration)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            _kernel = kernel;
		    _console = kernel.Resolve<IConsoleWriter>();
		    _configuration = configuration;

			Output("Starting");
            
			Execute(_configuration.OnStartup);

            _houseKeeping = new HouseKeeping(kernel, configuration, Output);

            Output("Started");
        }

        private void Output(string message)
		{
			_console.WriteLine($"[LiteServer]: {message}.");
		}

		public void Dispose()
		{
            Output("Stopping");

            _houseKeeping.Dispose();

			Execute(_configuration.OnShutdown);

			Output("Stopped");
        }

		private void Execute(IEnumerable<Action<IKernel>> actions)
		{
			foreach (Action<IKernel> action in actions)
			    action(_kernel);
		}
	}
}
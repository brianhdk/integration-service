using System;
using Microsoft.AspNet.SignalR.Hubs;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.WebApi.SignalR.Infrastructure
{
	internal class ErrorLoggingModule : HubPipelineModule
	{
		private readonly ILogger _logger;

		public ErrorLoggingModule(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException("logger");

			_logger = logger;
		}

		protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
		{
			_logger.LogError(exceptionContext.Error);
		}
	}
}
using System;
using System.Web.Http.ExceptionHandling;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.WebApi.Infrastructure
{
	internal class ExceptionLogger : System.Web.Http.ExceptionHandling.ExceptionLogger
	{
		private readonly ILogger _logger;

		public ExceptionLogger(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException("logger");

			_logger = logger;
		}

		public override void Log(ExceptionLoggerContext context)
		{
			_logger.LogError(context.Exception);
		}
	}
}
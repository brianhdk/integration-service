using System;
using System.Text;
using System.Web.Http.ExceptionHandling;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.WebApi.Infrastructure
{
	internal class ExceptionLogger : System.Web.Http.ExceptionHandling.ExceptionLogger
	{
		private readonly ILogger _logger;

		public ExceptionLogger(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			_logger = logger;
		}

		public override void Log(ExceptionLoggerContext context)
		{
			_logger.LogError(new UnhandledWebApiException(ConstructMessage(context.ExceptionContext), context.Exception));
		}

	    private static string ConstructMessage(ExceptionContext context)
	    {
	        var sb = new StringBuilder();

	        sb.AppendLine(context.Exception.DestructMessage());

	        sb.AppendLine();
	        sb.AppendLine("------");
	        sb.AppendLine();

	        sb.AppendLine($"Request: {context.Request}");

            if (context.ControllerContext?.ControllerDescriptor != null)
	            sb.AppendLine($"Controller: {context.ControllerContext.ControllerDescriptor.ControllerName}");

            if (context.ActionContext?.ActionDescriptor != null)
	            sb.AppendLine($"Action: {context.ActionContext.ActionDescriptor.ActionName}");

	        return sb.ToString();
	    }

	    private class UnhandledWebApiException : Exception
	    {
	        public UnhandledWebApiException(string message, Exception inner)
                : base(message, inner)
	        {
	        }
	    }
	}
}
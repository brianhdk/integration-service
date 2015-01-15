using System;
using System.Web.Http.Filters;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Web
{
    public class ExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        public ExceptionHandlingAttribute(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");

            _logger = logger;
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            _logger.LogError(context.Exception);
        }
    }
}
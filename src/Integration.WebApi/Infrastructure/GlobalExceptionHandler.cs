using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace Vertica.Integration.WebApi.Infrastructure
{
	internal class GlobalExceptionHandler : ExceptionHandler
	{
		public override void Handle(ExceptionHandlerContext context)
		{
			throw new NotImplementedException();
			if (context.Exception is ValidationException)
			{
				var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
				{
					Content = new StringContent(context.Exception.Message),
					ReasonPhrase = "ValidationException"
				};

				context.Result = new ErrorMessageResult(response);
			}
		}

		private class ErrorMessageResult : IHttpActionResult
		{
			private readonly HttpResponseMessage _httpResponseMessage;

			public ErrorMessageResult(HttpResponseMessage httpResponseMessage)
			{
				_httpResponseMessage = httpResponseMessage;
			}

			public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
			{
				return Task.FromResult(_httpResponseMessage);
			}
		}
	}
}
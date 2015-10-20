using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Vertica.Integration.Experiments.WebApi
{
	public class TestController : ApiController
	{
		public HttpResponseMessage Get(string name = null)
		{
			if (String.Equals(name, "invalid", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("asdf");

			if (String.Equals(name, "argument", StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException(@"Invalid name", "name");

			if (String.Equals(name, "httpexception", StringComparison.OrdinalIgnoreCase))
				throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "BadRequest"));

			if (String.Equals(name, "httpexception2", StringComparison.OrdinalIgnoreCase))
				throw new HttpResponseException(HttpStatusCode.BadRequest);

			if (String.Equals(name, "badrequest", StringComparison.OrdinalIgnoreCase))
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Bad request");

			return Request.CreateResponse(String.Format("Hello {0}", name ?? "N/A"));
		}
	}
}
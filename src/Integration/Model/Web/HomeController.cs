using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Vertica.Integration.Model.Web
{
    public class HomeController : ApiController
    {
        public HttpResponseMessage Get()
        {
            if (Request.Headers.Accept.Contains(new MediaTypeWithQualityHeaderValue("text/html")))
                return AssetsController.ServePortalFile(Request, "Default.html");

            return Request.CreateResponse(HttpStatusCode.OK, "Running.");
        }

        public HttpResponseMessage Put()
        {
            throw new InvalidOperationException("test");
        }
    }
}
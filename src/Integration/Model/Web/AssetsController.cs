using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Vertica.Integration.Model.Web
{
    public class AssetsController : ApiController
    {
        public HttpResponseMessage Get()
        {
            // find ud af hvilen fil der forespørges på - og returner denne...
            return Request.CreateResponse(HttpStatusCode.OK, "todo:get-the-file");
        }
    }
}
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Vertica.Integration.Model.Web
{
    public class HomeController : ApiController
    {
        public virtual HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Running.");
        }        
    }
}
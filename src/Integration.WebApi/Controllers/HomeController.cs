using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.WebApi.Controllers
{
    public class HomeController : ApiController
    {
        private readonly IUptime _uptime;

        public HomeController(IUptime uptime)
        {
            _uptime = uptime;
        }

        public virtual HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, $"Running. {_uptime}");
        }
    }
}
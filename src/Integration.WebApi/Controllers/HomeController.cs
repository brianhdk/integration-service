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

        public virtual IHttpActionResult Get()
        {
            return Ok($"Running. {_uptime.UptimeText}");
        }
    }
}
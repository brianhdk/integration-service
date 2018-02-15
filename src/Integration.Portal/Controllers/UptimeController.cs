using System.Web.Http;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Portal.Controllers
{
    public class UptimeController : ApiController
    {
        private readonly IUptime _uptime;

        public UptimeController(IUptime uptime)
        {
            _uptime = uptime;
        }

        public IHttpActionResult Get()
        {
            return Ok(new
            {
                StartedAt = _uptime.StartedAt.ToLocalTime().ToString("F"),
                Text = _uptime.UptimeText
            });
        }
    }
}
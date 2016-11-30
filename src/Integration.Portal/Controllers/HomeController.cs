using System.Net.Http;
using System.Net.Http.Headers;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Portal.Controllers
{
	public class HomeController : WebApi.Controllers.HomeController
    {
	    public HomeController(IUptime uptime) : base(uptime)
	    {
	    }

	    public override HttpResponseMessage Get()
        {
            if (Request.Headers.Accept.Contains(new MediaTypeWithQualityHeaderValue("text/html")))
                return AssetsController.ServeFile(Request, "Default.html");

            return base.Get();
        }
    }
}
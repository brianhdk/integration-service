using System.Net.Http;
using System.Net.Http.Headers;

namespace Integration.WebApi.SignalR.SmokeTester2
{
	public class HomeController : Vertica.Integration.WebApi.Controllers.HomeController
    {
        public override HttpResponseMessage Get()
        {
            if (Request.Headers.Accept.Contains(new MediaTypeWithQualityHeaderValue("text/html")))
                return AssetsController.ServeFile(Request, "Default.html");

            return base.Get();
        }
    }
}
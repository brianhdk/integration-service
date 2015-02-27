using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.Portal.Controllers
{
	class GraphController : ApiController
    {
        private readonly IDbFactory _dbFactory;

		public GraphController(IDbFactory dbFactory)
		{
			_dbFactory = dbFactory;
		}



		public HttpResponseMessage Get(int id)
		{
			return Request.CreateResponse(HttpStatusCode.OK);
		}
    }
}

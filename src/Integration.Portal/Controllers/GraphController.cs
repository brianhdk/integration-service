using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database.Dapper;

namespace Vertica.Integration.Portal.Controllers
{
	class GraphController : ApiController
    {
        private readonly IDapperProvider _dapper;

		public GraphController(IDapperProvider dapper)
		{
			_dapper = dapper;
		}

		public HttpResponseMessage Get(int id)
		{
			return Request.CreateResponse(HttpStatusCode.OK);
		}
    }
}

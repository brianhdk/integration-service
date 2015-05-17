using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.Portal.Controllers
{
	public class GraphController : ApiController
	{
		private readonly IDbFactory _db;

		public GraphController(IDbFactory db)
		{
			_db = db;
		}

		public HttpResponseMessage Get(int id)
		{
			switch (id)
			{
				case 1:
					return LastFiveDaysOfErrors();
				case 2:
					return PieChart();
			}

			return Request.CreateResponse(HttpStatusCode.NotFound);
		}

		private HttpResponseMessage PieChart()
		{
			return Request.CreateResponse(HttpStatusCode.NotImplemented);
		}

		private HttpResponseMessage LastFiveDaysOfErrors()
		{
			string sql = @"SELECT TOP 5 Max(CONVERT(VARCHAR(50), TimeStamp, 105)) as Date,
count(id) as Errors
FROM [ErrorLog]
group by DAY(TimeStamp)
order by Date desc";

			IEnumerable<object> errors;

			using (IDbSession session = _db.OpenSession())
			{
				errors = session.Query<object>(sql);
			}

			return Request.CreateResponse(HttpStatusCode.OK, errors);
		}
	}

	public enum Graph
	{
		Index = 1,
		Pie = 2
	}
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.Portal.Controllers
{
	public class GraphController : ApiController
	{
        private readonly Lazy<IDbFactory> _db;
        private readonly IIntegrationDatabaseConfiguration _configuration;

        public GraphController(Lazy<IDbFactory> db, IIntegrationDatabaseConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public HttpResponseMessage Get(int id)
        {
            if (_configuration.Disabled)
                return Request.CreateResponse(HttpStatusCode.OK);

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
			string sql = $@"
SELECT 
    TOP 5 Max(CONVERT(VARCHAR(50), TimeStamp, 105)) as Date,
    count(id) as Errors
FROM 
    [{_configuration.TableName(IntegrationDbTable.ErrorLog)}]
group by DAY(TimeStamp)
order by Date desc";

			IEnumerable<object> errors;

			using (IDbSession session = _db.Value.OpenSession())
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

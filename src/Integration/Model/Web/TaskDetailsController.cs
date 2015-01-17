using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Web
{
    public class TaskDetailsController : ApiController
	{
		private readonly IDbFactory _dbFactory;

		public TaskDetailsController(IDbFactory dbFactory)
	    {
		    _dbFactory = dbFactory;
	    }

	    public HttpResponseMessage Get()
        {
            var query = string.Format(@"
SELECT [TaskName]
FROM [TaskLog]
Group by [TaskName]
");

            IEnumerable<TaskLog> tasks;

			using (IDb db = _dbFactory.OpenDatabase())
			{
                tasks = db.Query<TaskLog>(query).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }

	    public HttpResponseMessage Get(string taskName)
        {
            var query = string.Format(@"
SELECT [TaskName]
FROM [TaskLog]
WHERE taskname = {0}
", taskName);

            IEnumerable<TaskLog> tasks;

			using (IDb db = _dbFactory.OpenDatabase())
			{
                tasks = db.Query<TaskLog>(query).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }
    }
}

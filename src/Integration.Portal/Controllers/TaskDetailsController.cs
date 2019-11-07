using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Portal.Controllers
{
    public class TaskDetailsController : ApiController
    {
        private readonly ITaskFactory _taskFactory;
        private readonly Lazy<IDbFactory> _db;
        private readonly IIntegrationDatabaseConfiguration _configuration;

        public TaskDetailsController(ITaskFactory taskFactory, Lazy<IDbFactory> db, IIntegrationDatabaseConfiguration configuration)
        {
            _taskFactory = taskFactory;
            _db = db;
            _configuration = configuration;
        }

        public HttpResponseMessage Get()
        {
            return 
                Request.CreateResponse(HttpStatusCode.OK,
                    _taskFactory.GetAll()
                        .Select(x => new { Name = x.Name(), x.Description }));
        }

        public HttpResponseMessage Get(string name)
        {
            ITask task = _taskFactory.Get(name);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Name = task.Name(),
                task.Description,
                Steps = task.Steps.Select(step => new
                {
                    Name = step.Name(),
                    step.Description
                }).ToArray()
            });
        }

        public HttpResponseMessage Get(string name, int count)
        {
            if (_configuration.Disabled)
                return Request.CreateResponse(HttpStatusCode.OK, new DateTimeOffset[0]);

            string sql = $@"
SELECT TOP {count}
	[TimeStamp]
FROM [{_configuration.TableName(IntegrationDbTable.TaskLog)}]
WHERE [TaskName] = '{name}' AND [Type] = 'T'
ORDER BY [TimeStamp] DESC
";

            IEnumerable<DateTimeOffset> lastRun;

            using (IDbSession session = _db.Value.OpenSession())
            {
                lastRun = session.Query<DateTimeOffset>(sql).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, lastRun);
        }
    }
}

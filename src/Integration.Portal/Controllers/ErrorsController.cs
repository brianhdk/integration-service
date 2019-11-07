using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Portal.Models;

namespace Vertica.Integration.Portal.Controllers
{
    public class ErrorsController : ApiController
    {
        private readonly Lazy<IDbFactory> _db;
        private readonly IIntegrationDatabaseConfiguration _configuration;

        public ErrorsController(Lazy<IDbFactory> db, IIntegrationDatabaseConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public HttpResponseMessage Get()
        {
            if (_configuration.Disabled)
                return Request.CreateResponse(HttpStatusCode.OK, new ErrorLogModel[0]);

            string sql = $@"
SELECT TOP 1000 [Id]
      ,[Message]
      ,[TimeStamp]
      ,[Severity]
      ,[Target]
FROM [{_configuration.TableName(IntegrationDbTable.ErrorLog)}] 
ORDER BY TimeStamp DESC
";

            IEnumerable<ErrorLogModel> errors;

            using (IDbSession session = _db.Value.OpenSession())
            {
                errors = session.Query<ErrorLogModel>(sql);
            }

            return Request.CreateResponse(HttpStatusCode.OK, errors);
        }

        public HttpResponseMessage Get(int id)
        {
            if (_configuration.Disabled)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not found.");

            string sql = $@"
SELECT [Id]
      ,[MachineName]
      ,[IdentityName]
      ,[CommandLine]
      ,[Message]
      ,[FormattedMessage]
      ,[TimeStamp]
      ,[Severity]
      ,[Target]
FROM [{_configuration.TableName(IntegrationDbTable.ErrorLog)}] 
WHERE [ID] = @id
";

            ErrorLogDetailedModel error;

            using (IDbSession session = _db.Value.OpenSession())
            {
                error = session.Query<ErrorLogDetailedModel>(sql, new { id }).SingleOrDefault();
            }

            return Request.CreateResponse(HttpStatusCode.OK, error);
        }
    }
}

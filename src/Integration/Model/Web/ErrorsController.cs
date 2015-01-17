using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Web
{
    public class ErrorsController : ApiController
    {
        private readonly IDbFactory _dbFactory;

        public ErrorsController(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public HttpResponseMessage Get()
        {
            var query = @"
SELECT [Id]
      ,[MachineName]
      ,[IdentityName]
      ,[CommandLine]
      ,[Message]
      ,[FormattedMessage]
      ,[TimeStamp]
      ,[Severity]
      ,[Target]
  FROM [ErrorLog]
";

            IEnumerable<ErrorLog> errors;

            using (var db = _dbFactory.OpenDatabase())
            {
                errors = db.Page<ErrorLog>(1, 20, query).Items;
                //errors = db.Query<ErrorLog>(query).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, errors);
        }

        public HttpResponseMessage Get(int id)
        {
            var query = @"
SELECT TOP 100 [Id]
      ,[MachineName]
      ,[IdentityName]
      ,[CommandLine]
      ,[Message]
      ,[FormattedMessage]
      ,[TimeStamp]
      ,[Severity]
      ,[Target]
  FROM [ErrorLog]
  WHERE [ID] = @0
";

            ErrorLog error;

            using (var db = _dbFactory.OpenDatabase())
            {
                error = db.SingleOrDefault<ErrorLog>(query, id);
            }

            return Request.CreateResponse(HttpStatusCode.OK, error);
        }
    }
}

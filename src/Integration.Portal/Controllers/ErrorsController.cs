using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Portal.Controllers
{
    public class ErrorsController : ApiController
    {
        private readonly IDbFactory _dbFactory;

        public ErrorsController(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public HttpResponseMessage Get(long page, long count)
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

            IPage<ErrorLog> pageResult;

            using (var db = _dbFactory.OpenDatabase())
            {
                pageResult = db.Page<ErrorLog>(page, count, query);
            }

            return Request.CreateResponse(HttpStatusCode.OK, pageResult);
        }

        public HttpResponseMessage Get(int id)
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

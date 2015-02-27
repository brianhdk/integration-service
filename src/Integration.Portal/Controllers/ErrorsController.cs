using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Portal.Controllers
{
    public class ErrorsController : ApiController
    {
        private readonly IDapperProvider _dapper;

        public ErrorsController(IDapperProvider dapper)
        {
            _dapper = dapper;
        }

        public HttpResponseMessage Get()
        {
            string sql = @"
SELECT [Id]
      ,[MachineName]
      ,[IdentityName]
      ,[CommandLine]
      ,[Message]
      ,[FormattedMessage]
      ,[TimeStamp]
      ,[Severity]
      ,[Target]
  FROM [ErrorLog] order by TimeStamp desc
";

            IEnumerable<ErrorLog> errors;

            using (IDapperSession session = _dapper.OpenSession())
            {
                errors = session.Query<ErrorLog>(sql);
            }

            return Request.CreateResponse(HttpStatusCode.OK, errors);
        }

        public HttpResponseMessage Get(int id)
        {
            string sql = @"
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
  WHERE [ID] = @0 order by TimeStamp desc
";

            ErrorLog error;

            using (IDapperSession session = _dapper.OpenSession())
            {
                error = session.Query<ErrorLog>(sql, new { id }).SingleOrDefault();
            }

            return Request.CreateResponse(HttpStatusCode.OK, error);
        }
    }
}

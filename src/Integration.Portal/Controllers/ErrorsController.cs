using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Portal.Models;

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
            const string sql = @"
SELECT [Id]
      ,[Message]
      ,[TimeStamp]
      ,[Severity]
      ,[Target]
  FROM [ErrorLog] ORDER BY TimeStamp DESC
";

            IEnumerable<ErrorLogModel> errors;

            using (IDapperSession session = _dapper.OpenSession())
            {
                errors = session.Query<ErrorLogModel>(sql);
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
  WHERE [ID] = @id
";

            ErrorLogDetailedModel error;

            using (IDapperSession session = _dapper.OpenSession())
            {
                error = session.Query<ErrorLogDetailedModel>(sql, new { id }).SingleOrDefault();
            }

            return Request.CreateResponse(HttpStatusCode.OK, error);
        }
    }
}

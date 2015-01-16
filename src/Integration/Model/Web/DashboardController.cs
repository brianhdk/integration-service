using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NHibernate;
using PetaPoco;
using Vertica.Integration.Infrastructure.Database.NHibernate;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Web
{
    public class DashboardController : ApiController
    {
        private readonly ISessionFactoryProvider _sessionFactory;

        public DashboardController(ISessionFactoryProvider sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public HttpResponseMessage Get()
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
  FROM [IC_PJ_Integration].[dbo].[ErrorLog]
  order by [ID] DESC
";

            IEnumerable<ErrorLog> errors;

            using (IStatelessSession session = _sessionFactory.SessionFactory.OpenStatelessSession())
            using (Database db = new PetaPoco.Database(session.Connection))
            {
                errors = db.Query<ErrorLog>(query).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, errors);
        }
    }
}

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Vertica.Integration.Domain.Monitoring;
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
            using (IStatelessSession session = _sessionFactory.SessionFactory.OpenStatelessSession())
            {
                ErrorLog errorLogAlias = null;
                TaskLog taskLogAlias = null;
                StepLog stepLogAlias = null;
                ExportIntegrationErrorsStep.ErrorEntry errorEntryAlias = null;

                IList<ExportIntegrationErrorsStep.ErrorEntry> errors =
                    session.QueryOver(() => errorLogAlias)
                            .SelectList(list => list
                                .Select(() => errorLogAlias.Id).WithAlias(() => errorEntryAlias.ErrorId)
                                .Select(() => errorLogAlias.Message).WithAlias(() => errorEntryAlias.ErrorMessage)
                                .Select(() => errorLogAlias.TimeStamp).WithAlias(() => errorEntryAlias.DateTime)
                                .Select(() => errorLogAlias.Severity).WithAlias(() => errorEntryAlias.Severity)
                                .Select(() => errorLogAlias.Target).WithAlias(() => errorEntryAlias.Target)
                                .SelectSubQuery(
                                    QueryOver.Of(() => taskLogAlias)
                                        .Where(taskLog => taskLog.ErrorLog.Id == errorLogAlias.Id)
                                        .Select(Projections.Property(() => taskLogAlias.TaskName))
                                ).WithAlias(() => errorEntryAlias.TaskNameFromTask)
                                .SelectSubQuery(
                                    QueryOver.Of(() => stepLogAlias)
                                        .Where(stepLog => stepLog.ErrorLog.Id == errorLogAlias.Id)
                                        .Select(Projections.Property(() => stepLogAlias.TaskName))
                                ).WithAlias(() => errorEntryAlias.TaskNameFromStep)
                                .SelectSubQuery(
                                    QueryOver.Of(() => stepLogAlias)
                                        .Where(stepLog => stepLog.ErrorLog.Id == errorLogAlias.Id)
                                        .Select(Projections.Property(() => stepLogAlias.StepName))
                                ).WithAlias(() => errorEntryAlias.StepName))
                            .OrderBy(() => errorLogAlias.Id).Desc
                            .TransformUsing(Transformers.AliasToBean<ExportIntegrationErrorsStep.ErrorEntry>())
                            .List<ExportIntegrationErrorsStep.ErrorEntry>();

                return Request.CreateResponse(HttpStatusCode.OK, errors);
            }
        }
    }
}

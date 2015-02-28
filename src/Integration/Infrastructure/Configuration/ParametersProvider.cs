using System;
using System.Data;
using System.Linq;
using Vertica.Integration.Infrastructure.Database.Dapper;

namespace Vertica.Integration.Infrastructure.Configuration
{
    public class ParametersProvider : IParametersProvider
    {
        private readonly IDapperProvider _dapper;

        public ParametersProvider(IDapperProvider dapper)
        {
            _dapper = dapper;
        }

        public Parameters Get()
        {
            using (IDapperSession session = _dapper.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                var parameters =
                    session.Query<Parameters>(@"SELECT [Id], [LastMonitorCheck] FROM [dbo].[Parameters]").SingleOrDefault();

                if (parameters == null)
                {
                    parameters = new Parameters();
                    session.Execute(
                        @"INSERT INTO [dbo].[Parameters] ([Id], [LastMonitorCheck]) VALUES (@Id, @LastMonitorCheck)",
                        new
                        {
                            Id = parameters.Id,
                            LastMonitorCheck = parameters.LastMonitorCheck
                        });
                    transaction.Commit();
                }

                return parameters;
            }
        }

        public void Save(Parameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");

            using (var session = _dapper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.Execute(
                    @"UPDATE [dbo].[Parameters] SET [LastMonitorCheck] = @LastMonitorCheck WHERE [Id] = @Id",
                    new
                    {
                        Id = parameters.Id,
                        LastMonitorCheck = parameters.LastMonitorCheck
                    });

                transaction.Commit();
            }
        }
    }
}
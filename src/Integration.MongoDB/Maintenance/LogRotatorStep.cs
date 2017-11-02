using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;
using Vertica.Integration.MongoDB.Commands;
using Vertica.Integration.MongoDB.Infrastructure;

namespace Vertica.Integration.MongoDB.Maintenance
{
    public class LogRotatorStep<TConnection> : Step<MaintenanceWorkItem>
        where TConnection : Connection
    {
        private readonly IMongoDbClientFactory<TConnection> _db;
        private readonly ILogRotatorCommand _logRotator;

        public LogRotatorStep(IMongoDbClientFactory<TConnection> db, ILogRotatorCommand logRotator)
        {
            _db = db;
            _logRotator = logRotator;
        }

        public override void Execute(ITaskExecutionContext<MaintenanceWorkItem> context)
        {
            _logRotator.Execute(_db.Client, context.CancellationToken);
        }

        public override string Description => "Performs a logRotate command against the MongoDB server.";
    }

    public class LogRotatorStep : LogRotatorStep<DefaultConnection>
    {
        public LogRotatorStep(IMongoDbClientFactory<DefaultConnection> db, ILogRotatorCommand logRotator)
            : base(db, logRotator)
        {
        }
    }
}
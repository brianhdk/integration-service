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
        private readonly ILogRotatorCommand _command;

        public LogRotatorStep(IMongoDbClientFactory<TConnection> db, ILogRotatorCommand command)
        {
            _db = db;
            _command = command;
        }

        public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
            _command.Execute(_db.Client);
        }

        public override string Description => "Performs a logRotate command against the MongoDB server.";
    }

    public class LogRotatorStep : LogRotatorStep<DefaultConnection>
    {
        public LogRotatorStep(IMongoDbClientFactory<DefaultConnection> db, ILogRotatorCommand command)
            : base(db, command)
        {
        }
    }
}
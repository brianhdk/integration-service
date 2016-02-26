using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;
using Vertica.Integration.MongoDB.Commands;

namespace Vertica.Integration.MongoDB.Maintenance
{
    public class LogRotatorStep : Step<MaintenanceWorkItem>
    {
        private readonly IMongoDbClientFactory _db;
        private readonly ILogRotatorCommand _command;

        [Obsolete("Don't use.")]
        public LogRotatorStep()
        {
            throw new InvalidOperationException(@"Missing required configuration of MongoDB. 
Make sure to use the ""UseMongoDb()"" Extension Method and make sure to specify the Connection.

application
    .MongoDB(mongoDB => mongoDB
        // http://docs.mongodb.org/manual/reference/connection-string/#connections-connection-options
        .Connection(ConnectionString.FromText(""mongodb://[username:password@]host1[:port1][,host2[:port2],...[,hostN[:portN]]][/[database][?options]]""");
        }

        public LogRotatorStep(IMongoDbClientFactory db, ILogRotatorCommand command)
        {
            _db = db;
            _command = command;
        }

        public override string Description => "Performs a logRotate command against the MongoDB server.";

	    public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
            _command.Execute(_db.Client);
        }
    }
}
using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.MongoDB.Commands
{
    public class LogRotatorCommand : ILogRotatorCommand
    {
        private readonly IShutdown _shutdown;

        public LogRotatorCommand(IShutdown shutdown)
        {
            _shutdown = shutdown;
        }

        public void Execute(IMongoClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            Execute(client.GetDatabase("admin")).Wait(_shutdown.Token);
        }

        private Task Execute(IMongoDatabase database)
        {
            var command = new BsonDocumentCommand<dynamic>(new BsonDocument
            {
                { "logRotate", 1}
            });

            return database.RunCommandAsync(command, cancellationToken: _shutdown.Token);
        }
    }
}
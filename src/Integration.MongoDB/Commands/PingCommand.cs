using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Vertica.Integration.MongoDB.Commands
{
    public class PingCommand : IPingCommand
    {
        public void Execute(IMongoClient client, CancellationToken token)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            Execute(client.GetDatabase("admin"), token).Wait(token);
        }

        private Task Execute(IMongoDatabase database, CancellationToken token)
        {
            var command = new BsonDocumentCommand<dynamic>(new BsonDocument
            {
                { "ping", 1}
            });

            return database.RunCommandAsync(command, cancellationToken: token);
        }
    }
}
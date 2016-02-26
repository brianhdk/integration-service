using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Vertica.Integration.MongoDB.Commands
{
    public class LogRotatorCommand : ILogRotatorCommand
    {
        public void Execute(IMongoClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            Execute(client.GetDatabase("admin")).Wait();
        }

        private async Task Execute(IMongoDatabase database)
        {
            var command = new BsonDocumentCommand<dynamic>(new BsonDocument
            {
                { "logRotate", 1}
            });

            await database.RunCommandAsync(command);
        }
    }
}
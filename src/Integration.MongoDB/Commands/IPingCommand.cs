using System.Threading;
using MongoDB.Driver;

namespace Vertica.Integration.MongoDB.Commands
{
    public interface IPingCommand
    {
        void Execute(IMongoClient client, CancellationToken token);
    }
}
using System.Threading;
using MongoDB.Driver;

namespace Vertica.Integration.MongoDB.Commands
{
    public interface ILogRotatorCommand
    {
        void Execute(IMongoClient client);
    }
}
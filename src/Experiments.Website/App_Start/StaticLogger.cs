using System.Collections.Concurrent;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;

namespace Experiments.Website
{
    public class StaticLogger : Logger
    {
        public static ConcurrentQueue<ErrorLog> Exceptions = new ConcurrentQueue<ErrorLog>();

        protected override string Insert(TaskLog log)
        {
            return null;
        }

        protected override string Insert(MessageLog log)
        {
            return null;
        }

        protected override string Insert(StepLog log)
        {
            return null;
        }

        protected override string Insert(ErrorLog log)
        {
            Exceptions.Enqueue(log);

            return null;
        }

        protected override void Update(TaskLog log)
        {
        }

        protected override void Update(StepLog log)
        {
        }
    }
}
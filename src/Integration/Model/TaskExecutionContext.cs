using System;
using System.Collections.Generic;
using System.Threading;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Model
{
    internal class TaskExecutionContext : ITaskExecutionContext
    {
        private readonly IDictionary<string, object> _context;

        public TaskExecutionContext(ILog log, Arguments arguments, CancellationToken cancellationToken)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
	        if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            _context = new Dictionary<string, object>();

            StartTimeUtc = Time.UtcNow;
            Log = log;
            Arguments = arguments;
            CancellationToken = cancellationToken;
        }

        public DateTimeOffset StartTimeUtc { get; }
        public ILog Log { get; }
        public Arguments Arguments { get; }

        public CancellationToken CancellationToken { get; }

        public void ThrowIfCancelled()
        {
            CancellationToken.ThrowIfCancellationRequested();
        }

        public object this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

                object value;
                _context.TryGetValue(name, out value);
                return value;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

                _context[name] = value;
            }
        }
    }
}
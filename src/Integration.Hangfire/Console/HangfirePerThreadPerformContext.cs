using System;
using Hangfire.Server;

namespace Vertica.Integration.Hangfire.Console
{
    internal class HangfirePerThreadPerformContext
    {
        private PerformContext _context;

        public PerformContext Get()
        {
            return _context;
        }

        public void Set(PerformContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _context = context;
        }
    }
}
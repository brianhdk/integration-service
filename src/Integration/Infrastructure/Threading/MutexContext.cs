using System;
using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Threading
{
    /*
    public class MutexContext
    {
        private readonly Dictionary<string, object> _context;

        public MutexContext(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty", nameof(name));

            Name = name;
            _context = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }

        public MutexContext WaitTimeBetweenRetries(TimeSpan waitTime)
        {
            return this;
        }

        public MutexContext Retry(int count)
        {
            return this;
        }

        public MutexContext WithContext(string key, object context)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(@"Value cannot be null or empty", nameof(key));

            _context.Add(key, context);
            
            return this;
        }

        public object this[string key]
        {
            get
            {
                object context;
                _context.TryGetValue(key, out context);
                return context;
            }
        }
    }
    */
}
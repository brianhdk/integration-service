using System;
using System.Collections.Generic;

namespace Vertica.Integration.Model
{
    public abstract class ContextWorkItem
    {
        private readonly IDictionary<string, object> _context;

        protected ContextWorkItem()
        {
            _context = new Dictionary<string, object>();
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

        public T Context<T>(string name, T context = null) where T : class
        {
            if (context != null)
            {
                this[name] = context;
                return context;
            }

            return this[name] as T;
        }
    }
}
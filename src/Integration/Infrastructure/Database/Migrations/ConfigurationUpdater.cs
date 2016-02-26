using System;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public class ConfigurationUpdater<T> : IDisposable where T : class, new()
    {
        private readonly Action<T> _save;

        public T Configuration { get; private set; }

        internal ConfigurationUpdater(T configuration, Action<T> save)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (save == null) throw new ArgumentNullException(nameof(save));

            Configuration = configuration;
            _save = save;
        }

        public void Dispose()
        {
            _save(Configuration);
        }
    }
}
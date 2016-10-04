using System;
using System.Collections.Concurrent;

namespace Vertica.Integration.Infrastructure.Features
{
    public class FeatureToggler : IFeatureToggler
    {
        private readonly ConcurrentDictionary<Type, bool> _disabled;

        public FeatureToggler()
        {
            _disabled = new ConcurrentDictionary<Type, bool>();
        }

        public void Disable<TFeature>()
        {
            _disabled.AddOrUpdate(typeof(TFeature), x => true, (x, current) => true);
        }

        public void Enable<TFeature>()
        {
            _disabled.AddOrUpdate(typeof(TFeature), x => false, (x, current) => false);
        }

        public bool IsDisabled<TFeature>()
        {
            bool value;
            if (_disabled.TryGetValue(typeof(TFeature), out value))
                return value;

            return false;
        }
    }
}
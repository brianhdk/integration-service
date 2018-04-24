using System;
using System.Collections.Generic;
using InvalidOperationException = System.InvalidOperationException;

namespace Vertica.Integration
{
    public class EnvironmentConfiguration
    {
        private readonly Dictionary<ApplicationEnvironment, List<Action<ApplicationConfiguration>>> _environments;
        private readonly List<Action<ApplicationConfiguration>> _fallbacks;

        private ApplicationEnvironment _current;

        internal EnvironmentConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            _environments = new Dictionary<ApplicationEnvironment, List<Action<ApplicationConfiguration>>>(3);
            _fallbacks = new List<Action<ApplicationConfiguration>>(1);
            _current = new AppConfigRuntimeSettings().Environment;

            Application = application;
        }

        public ApplicationConfiguration Application { get; }

        public EnvironmentConfiguration Customize(ApplicationEnvironment environment, Action<ApplicationConfiguration> customization)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (customization == null) throw new ArgumentNullException(nameof(customization));

            if (!_environments.TryGetValue(environment, out List<Action<ApplicationConfiguration>> customizations))
                customizations = _environments[environment] = new List<Action<ApplicationConfiguration>>(1);

            customizations.Add(customization);
            
            return this;
        }

        public EnvironmentConfiguration Fallback(Action<ApplicationConfiguration> fallback)
        {
            if (fallback == null) throw new ArgumentNullException(nameof(fallback));

            _fallbacks.Add(fallback);

            return this;
        }

        /// <summary>
        /// By default the current environment will be read from the current Application Configuration File (.config) using the built-in <see cref="System.Configuration.ConfigurationManager"/> class (AppSettings).
        /// Use this method to override that to behaviour.
        /// </summary>
        public EnvironmentConfiguration OverrideCurrent(ApplicationEnvironment environment)
        {
            _current = environment ?? throw new ArgumentNullException(nameof(environment));

            return this;
        }

        internal void Apply()
        {
            if (_current == null)
            {
                if (_environments.Count > 0 || _fallbacks.Count > 0)
                {
                    throw new InvalidOperationException($@"Unable to apply any environment specific customizations.

The current environment is null.

Set the current environment on the <appSettings>-element in Application Configuration file for this host (app.config or web.config), e.g.: 
  <add key=""Environment"" value=""Development"" />

... or specify the current environment explicitely using {nameof(OverrideCurrent)} method on this type.");
                }

                return;
            }

            foreach (Action<ApplicationConfiguration> customization in CustomizationsFor(_current))
                customization(Application);
        }

        private IEnumerable<Action<ApplicationConfiguration>> CustomizationsFor(ApplicationEnvironment environment)
        {
            if (_environments.TryGetValue(environment, out List<Action<ApplicationConfiguration>> customizations))
                return customizations;

            return _fallbacks;
        }
    }
}
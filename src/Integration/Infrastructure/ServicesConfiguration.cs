using System;

namespace Vertica.Integration.Infrastructure
{
    public class ServicesConfiguration
    {
        private readonly ServicesInterceptorsConfiguration _interceptors;
        private readonly ServicesAdvancedConfiguration _advanced;
        private readonly ServicesConventionsConfiguration _conventions;

        internal ServicesConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            _interceptors = new ServicesInterceptorsConfiguration(this);
            _advanced = new ServicesAdvancedConfiguration(this);
            _conventions = new ServicesConventionsConfiguration(this);

            Application = application
                // NOTE: Interceptors has to go first.
                .Extensibility(extensibility => extensibility.Register(() => _interceptors))
                // NOTE: Advanced has to go second as they take precedence over the conventional registrations.
                .Extensibility(extensibility => extensibility.Register(() => _advanced))
                .Extensibility(extensibility => extensibility.Register(() => _conventions));
        }

        public ApplicationConfiguration Application { get; }

        public ServicesConfiguration Interceptors(Action<ServicesInterceptorsConfiguration> interceptors)
        {
            interceptors?.Invoke(_interceptors);

            return this;
        }

        public ServicesConfiguration Advanced(Action<ServicesAdvancedConfiguration> advanced)
        {
            advanced?.Invoke(_advanced);

            return this;
        }

        public ServicesConfiguration Conventions(Action<ServicesConventionsConfiguration> conventions)
        {
            conventions?.Invoke(_conventions);

            return this;
        }
    }
}
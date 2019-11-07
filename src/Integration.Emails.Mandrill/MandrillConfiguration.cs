using System;
using System.Net.Http;
using Castle.MicroKernel;
using Vertica.Integration.Emails.Mandrill.Infrastructure;
using Vertica.Integration.Emails.Mandrill.Infrastructure.Providers;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Emails.Mandrill
{
	public class MandrillConfiguration : IInitializable<ApplicationConfiguration>
	{
	    private Type _configurationProvider;
	    private readonly State _state;
	    private bool _replaceEmailService;

	    internal MandrillConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

            _state = new State();

		    Application = application;
        }

	    public ApplicationConfiguration Application { get; }

        public MandrillConfiguration ConfiguredBy(Action<ConfiguredByProvider> by)
        {
            var provider = new ConfiguredByProvider(x => _configurationProvider = x);

            by(provider);

            return this;
        }

        public class ConfiguredByProvider
        {
            private readonly Action<Type> _provider;

            public ConfiguredByProvider(Action<Type> provider)
            {
                _provider = provider;
            }

            public void RuntimeSettings()
            {
                _provider(typeof(RuntimeSettingsMandrillSettingsProvider));
            }

            public void Using<TProvider>()
                where TProvider : IMandrillSettingsProvider
            {
                _provider(typeof(TProvider));
            }
        }

	    public MandrillConfiguration WithHttpMessageHandler(Func<IKernel, HttpMessageHandler> handler)
	    {
	        if (handler == null) throw new ArgumentNullException(nameof(handler));

	        _state.HttpMessageHandler = handler;

	        return this;
	    }

	    public MandrillConfiguration ReplaceEmailService()
	    {
	        _replaceEmailService = true;

	        return this;
	    }

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
        {
            application.Services(services => services
                .Conventions(conventions => conventions
                    .AddFromAssemblyOfThis<MandrillConfiguration>())
                .Advanced(advanced =>
                {
                    if (_configurationProvider != null)
                    {
                        advanced.Install(Install.Type(typeof(IMandrillSettingsProvider), _configurationProvider));
                    }
                    else
                    {
                        advanced.Register<IMandrillSettingsProvider, ConfigurationServiceMandrillSettingsProvider>();
                    }

                    advanced.Install(Install.Instance(_state));

                    advanced.Register<IMandrillApiService, MandrillApiService>();

                    if (_replaceEmailService)
                        advanced.Register<IEmailService, MandrillEmailService>();
                })
            );
        }

	    internal class State
	    {
	        public Func<IKernel, HttpMessageHandler> HttpMessageHandler { get; set; }
        }
	}
}
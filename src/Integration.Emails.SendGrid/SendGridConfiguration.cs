using System;
using System.Net.Http;
using Castle.MicroKernel;
using Vertica.Integration.Emails.SendGrid.Infrastructure;
using Vertica.Integration.Emails.SendGrid.Infrastructure.Providers;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Emails.SendGrid
{
	public class SendGridConfiguration : IInitializable<ApplicationConfiguration>
	{
	    private Type _configurationProvider;
	    private readonly State _state;
	    private bool _replaceEmailService;

	    internal SendGridConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

            _state = new State();

		    Application = application;
        }

	    public ApplicationConfiguration Application { get; }

        public SendGridConfiguration ConfiguredBy(Action<ConfiguredByProvider> by)
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
                _provider(typeof(RuntimeSettingsSendGridSettingsProvider));
            }

            public void Using<TProvider>()
                where TProvider : ISendGridSettingsProvider
            {
                _provider(typeof(TProvider));
            }
        }

	    public SendGridConfiguration ReplaceEmailService()
	    {
	        _replaceEmailService = true;

	        return this;
	    }

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
        {
            application.Services(services => services
                .Conventions(conventions => conventions
                    .AddFromAssemblyOfThis<SendGridConfiguration>())
                .Advanced(advanced =>
                {
                    if (_configurationProvider != null)
                    {
                        advanced.Install(Install.Type(typeof(ISendGridSettingsProvider), _configurationProvider));
                    }
                    else
                    {
                        advanced.Register<ISendGridSettingsProvider, ConfigurationServiceSendGridSettingsProvider>();
                    }

                    advanced.Install(Install.Instance(_state));

                    if (_replaceEmailService)
                        advanced.Register<IEmailService, SendGridEmailService>();
                })
            );
        }

	    internal class State
	    {
	        public Func<IKernel, HttpMessageHandler> HttpMessageHandler { get; set; }
        }
	}
}
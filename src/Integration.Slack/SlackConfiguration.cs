using System;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Slack.Bot;
using Vertica.Integration.Slack.LiteServer;
using Vertica.Integration.Slack.Messaging;
using Vertica.Utilities.Extensions.StringExt;

namespace Vertica.Integration.Slack
{
	public class SlackConfiguration : IInitializable<ApplicationConfiguration>
	{
	    private readonly SlackMessageHandlersConfiguration _messageHandlers;
	    private readonly SlackBotCommandsConfiguration _botCommands;

	    private bool _attachToConsoleWriter;

	    internal SlackConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

		    Application = application
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<ISlackConfiguration, ConfigurationImpl>())
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<SlackConfiguration>()));

            _messageHandlers = new SlackMessageHandlersConfiguration(this);
		    Application.Extensibility(extensibility => extensibility.Register(() => _messageHandlers));

            _botCommands = new SlackBotCommandsConfiguration(this);
            Application.Extensibility(extensibility => extensibility.Register(() => _botCommands));
        }

	    public ApplicationConfiguration Application { get; }

	    public SlackConfiguration Change(Action<SlackConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}

	    /// <summary>
		/// Adds Slack to <see cref="ILiteServerFactory"/> allowing Slack to run simultaneously with other servers.
		/// </summary>
		public SlackConfiguration AddToLiteServer()
		{
			Application.UseLiteServer(liteServer => liteServer
                .AddServer<SlackMessageQueueConsumerServer>()
                .AddServer<SlackBotWorkerServer>());

			return this;
		}

	    public SlackConfiguration AttachToConsoleWriter()
	    {
	        _attachToConsoleWriter = true;

	        return this;
	    }

	    public SlackConfiguration MessageHandlers(Action<SlackMessageHandlersConfiguration> messageHandlers)
	    {
	        messageHandlers?.Invoke(_messageHandlers);

	        return this;
	    }

        public SlackConfiguration BotCommands(Action<SlackBotCommandsConfiguration> botCommands)
        {
            botCommands?.Invoke(_botCommands);

            return this;
        }
        
	    void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
	    {
	        if (_attachToConsoleWriter)
	        {
	            application.Services(services => services
	                .Interceptors(interceptors => interceptors
	                    .InterceptService<IConsoleWriter, SlackConsoleWriterInterceptor>()));
	        }
	    }

        internal class ConfigurationImpl : ISlackConfiguration
        {
            private readonly IRuntimeSettings _runtimeSettings;

            public ConfigurationImpl(IRuntimeSettings runtimeSettings)
            {
                _runtimeSettings = runtimeSettings;
            }

            public bool Enabled
            {
                get
                {
                    bool enabled;
                    bool.TryParse(_runtimeSettings[$"Slack.{nameof(Enabled)}"], out enabled);

                    return enabled;
                }
            }

            public string BotUserToken
            {
                get
                {
                    string key = $"Slack.{nameof(BotUserToken)}";

                    string token = _runtimeSettings[key];

                    if (string.IsNullOrWhiteSpace(token) && Enabled)
                        throw new InvalidOperationException($@"Token for Bot User has not been specified. 
Make sure that you've created a Slack Bot User, and then copy the token into the following setting: {key}");

                    return token;
                }
            }

            public string DefaultChannel => _runtimeSettings[$"Slack.{nameof(DefaultChannel)}"].NullIfEmpty() ?? "#general";
        }
    }
}
using System;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Slack.Bot;
using Vertica.Integration.Slack.Infrastructure.LiteServer;
using Vertica.Integration.Slack.Messaging;

namespace Vertica.Integration.Slack
{
	public class SlackConfiguration : IInitializable<ApplicationConfiguration>
	{
	    private readonly SlackMessageHandlersConfiguration _messageHandlers;
	    private readonly SlackBotConfiguration _bot;

	    private bool _attachToConsoleWriter;

	    internal SlackConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

		    Application = application
                .Services(services => services
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<SlackConfiguration>()));

            _messageHandlers = new SlackMessageHandlersConfiguration(this);
		    Application.Extensibility(extensibility => extensibility.Register(() => _messageHandlers));

            _bot = new SlackBotConfiguration(this);
            Application.Extensibility(extensibility => extensibility.Register(() => _bot));
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
			Application.UseLiteServer(server => server
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

        public SlackConfiguration Bot(Action<SlackBotConfiguration> bot)
        {
            bot?.Invoke(_bot);

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
	}
}
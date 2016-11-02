using System;
using Vertica.Integration.Slack.Infrastructure.Castle.Windsor;
using Vertica.Integration.Slack.Messaging.Handlers;

namespace Vertica.Integration.Slack.Messaging
{
	public class SlackMessageHandlersConfiguration
	{
		private readonly MessageHandlersInstaller _installer;
		
		internal SlackMessageHandlersConfiguration(SlackConfiguration slack)
		{
			if (slack == null) throw new ArgumentNullException(nameof(slack));

			_installer = new MessageHandlersInstaller();

		    Slack = slack.Change(s => s
		        .Application.Services(services => services
		            .Advanced(advanced => advanced
		                .Install(_installer))));
		}

		public SlackConfiguration Slack { get; }

		/// <summary>
		/// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="IHandleMessages{T}"/>.
		/// <para />
		/// </summary>
		/// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
		public SlackMessageHandlersConfiguration AddFromAssemblyOfThis<T>()
		{
			_installer.AddFromAssemblyOfThis<T>();

			return this;
		}

        /// <summary>
        /// Adds the specified <typeparamref name="THandler"/>.
        /// </summary>
        /// <typeparam name="THandler">Specifies the <see cref="IHandleMessages"/> to be added.</typeparam>
        public SlackMessageHandlersConfiguration Add<THandler>()
			where THandler : IHandleMessages
		{
			_installer.Add<THandler>();

			return this;
		}

		/// <summary>
		/// Skips the specified <typeparamref name="THandler" />.
		/// </summary>
		/// <typeparam name="THandler">Specifies the <see cref="IHandleMessages"/> that will be skipped.</typeparam>
		public SlackMessageHandlersConfiguration Remove<THandler>()
			where THandler : IHandleMessages
        {
			_installer.Remove<THandler>();

			return this;
		}

		/// <summary>
		/// Clears all registred <see cref="IHandleMessages{T}"/>.
		/// </summary>
		public SlackMessageHandlersConfiguration Clear()
		{
			_installer.Clear();

			return this;
		}
	}
}
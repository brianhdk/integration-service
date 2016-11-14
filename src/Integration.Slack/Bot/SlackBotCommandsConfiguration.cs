using System;
using Vertica.Integration.Slack.Bot.Commands;
using Vertica.Integration.Slack.Bot.Commands.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.Slack.Bot
{
	public class SlackBotCommandsConfiguration
	{
		private readonly SlackBotCommandsInstaller _installer;

        internal SlackBotCommandsConfiguration(SlackConfiguration slack)
        {
            if (slack == null) throw new ArgumentNullException(nameof(slack));

            _installer = new SlackBotCommandsInstaller();

            Slack = slack.Change(s => s
                .Application.Services(services => services
                    .Advanced(advanced => advanced
                        .Install(_installer))));
        }

        public SlackConfiguration Slack { get; }

        /// <summary>
        /// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="ISlackBotCommand"/>.
        /// <para />
        /// </summary>
        /// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
        public SlackBotCommandsConfiguration AddFromAssemblyOfThis<T>()
		{
			_installer.AddFromAssemblyOfThis<T>();

			return this;
		}

        /// <summary>
        /// Adds the specified <typeparamref name="TCommand"/>.
        /// </summary>
        /// <typeparam name="TCommand">Specifies the <see cref="ISlackBotCommand"/> to be added.</typeparam>
        public SlackBotCommandsConfiguration Add<TCommand>()
			where TCommand : ISlackBotCommand
        {
			_installer.Add<TCommand>();

			return this;
		}

        /// <summary>
        /// Skips the specified <typeparamref name="TCommand" />.
        /// </summary>
        /// <typeparam name="TCommand">Specifies the <see cref="ISlackBotCommand"/> that will be skipped.</typeparam>
        public SlackBotCommandsConfiguration Remove<TCommand>()
			where TCommand : ISlackBotCommand
        {
			_installer.Remove<TCommand>();

			return this;
		}

		/// <summary>
		/// Clears all registrations.
		/// </summary>
		public SlackBotCommandsConfiguration Clear()
		{
			_installer.Clear();

			return this;
		}
	}
}
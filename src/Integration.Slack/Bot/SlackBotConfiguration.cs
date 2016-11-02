using System;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Slack.Bot
{
	public class SlackBotConfiguration
	{
		private readonly ScanAddRemoveInstaller<IBackgroundWorker> _workers;

        internal SlackBotConfiguration(SlackConfiguration slack)
        {
            if (slack == null) throw new ArgumentNullException(nameof(slack));

            _workers = new ScanAddRemoveInstaller<IBackgroundWorker>();

            Slack = slack.Change(s => s
                .Application.Services(services => services
                    .Advanced(advanced => advanced
                        .Install(_workers))));
        }

        public SlackConfiguration Slack { get; }

		/// <summary>
		/// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="IBackgroundServer"/> and/or <see cref="IBackgroundWorker"/>
		/// <para />
		/// </summary>
		/// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
		public SlackBotConfiguration AddFromAssemblyOfThis<T>()
		{
			_workers.AddFromAssemblyOfThis<T>();

			return this;
		}

		/// <summary>
		/// Adds the specified <typeparamref name="TWorker"/>.
		/// </summary>
		/// <typeparam name="TWorker">Specifies the <see cref="IBackgroundWorker"/> to be added.</typeparam>
		public SlackBotConfiguration AddWorker<TWorker>()
			where TWorker : IBackgroundWorker
		{
			_workers.Add<TWorker>();

			return this;
		}

		/// <summary>
		/// Skips the specified <typeparamref name="TWorker" />.
		/// </summary>
		/// <typeparam name="TWorker">Specifies the <see cref="IBackgroundWorker"/> that will be skipped.</typeparam>
		public SlackBotConfiguration RemoveWorker<TWorker>()
			where TWorker : IBackgroundWorker
		{
			_workers.Remove<TWorker>();

			return this;
		}

		/// <summary>
		/// Clears all registrations.
		/// </summary>
		public SlackBotConfiguration Clear()
		{
			_workers.Clear();

			return this;
		}
	}
}
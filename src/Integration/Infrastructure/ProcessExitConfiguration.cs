using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Infrastructure
{
	public class ProcessExitConfiguration : IInitializable<IWindsorContainer>
	{
		private IProcessExitHandler _customInstance;
		private Type _customType;

		public ProcessExitConfiguration(AdvancedConfiguration advanced)
		{
			if (advanced == null) throw new ArgumentNullException(nameof(advanced));

			Advanced = advanced;
		}

		public AdvancedConfiguration Advanced { get; }

		/// <summary>
		/// Registers a custom handler to take care of notifying Integration Service when to shut down.
		/// </summary>
		public ProcessExitConfiguration Custom<TCustomHandler>()
			where TCustomHandler : class, IProcessExitHandler
		{
			_customInstance = null;
			_customType = typeof (TCustomHandler);

			return this;
		}

		/// <summary>
		/// Registers a custom handler to take care of notifying Integration Service when to shut down.
		/// </summary>
		public ProcessExitConfiguration Custom(IProcessExitHandler customHandler)
		{
			if (customHandler == null) throw new ArgumentNullException(nameof(customHandler));

			_customInstance = customHandler;
			_customType = null;

			return this;
		}

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			if (_customType != null)
			{
				container.Install(Install.Type<IProcessExitHandler>(_customType));
			}
			else if (_customInstance != null)
			{
				container.Install(Install.Instance(_customInstance));
			}
			else if (AzureWebJobShutdownHandler.IsRunningOnAzure())
			{
				container.Install(Install.Service<IProcessExitHandler, AzureWebJobShutdownHandler>());
			}
			else
			{
				container.Install(Install.Service<IProcessExitHandler, DefaultHandler>());
			}
		}
	}
}
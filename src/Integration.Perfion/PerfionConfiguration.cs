﻿using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Perfion
{
	public class PerfionConfiguration : IPerfionConfiguration, IInitializable<IWindsorContainer>
	{
		internal PerfionConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException("application");

			Application = application
				.AddCustomInstaller(Install.ByConvention
					.AddFromAssemblyOfThis<IPerfionService>()
					.Ignore<PerfionConfiguration>());
		}

		public ConnectionString ConnectionString { get; set; }

		public ArchiveOptions ArchiveOptions { get; private set; }

		public PerfionConfiguration Change(Action<PerfionConfiguration> change)
		{
			if (change != null)
				change(this);

			return this;
		}

		/// <summary>
		/// Will create an archive when querying for data in Perfion.
		/// </summary>
		public PerfionConfiguration EnableArchiving(Action<ArchiveOptions> options = null)
		{
			ArchiveOptions = new ArchiveOptions("Catalog").GroupedBy("Perfion").ExpiresAfterMonths(1);

			if (options != null)
				options(ArchiveOptions);

			return this;
		}

		public ApplicationConfiguration Application { get; private set; }

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			if (ConnectionString == null)
				ConnectionString = ConnectionString.FromName("Perfion.APIService.Url");

			container.RegisterInstance<IPerfionConfiguration>(this);
		}
	}
}
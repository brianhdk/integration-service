using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Globase
{
	public class GlobaseConfiguration : IInitializable<IWindsorContainer>
	{
		internal GlobaseConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.AddCustomInstaller(Install.ByConvention
					.AddFromAssemblyOfThis<GlobaseConfiguration>()
					.Ignore<GlobaseConfiguration>());
		}

		internal ConnectionString FtpConnectionStringInternal { get; set; }
		
		public ArchiveOptions ArchiveOptions { get; private set; }

		public GlobaseConfiguration FtpConnectionString(ConnectionString ftp)
		{
			if (ftp == null) throw new ArgumentNullException(nameof(ftp));

			FtpConnectionStringInternal = ftp;

			return this;
		}

		public GlobaseConfiguration Change(Action<GlobaseConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}

		/// <summary>
		/// Will create an archive when uploading data to Globase.
		/// </summary>
		public GlobaseConfiguration EnableArchiving(Action<ArchiveOptions> options = null)
		{
			ArchiveOptions = new ArchiveOptions("Globase").GroupedBy("Globase").ExpiresAfterMonths(1);

			options?.Invoke(ArchiveOptions);

			return this;
		}

		public ApplicationConfiguration Application { get; private set; }

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			if (FtpConnectionStringInternal == null)
				FtpConnectionStringInternal = ConnectionString.FromName("Globase.Ftp.Url");

			container.RegisterInstance(this);
		}
	}
}
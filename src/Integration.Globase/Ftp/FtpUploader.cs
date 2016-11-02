using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Infrastructure.Remote.Ftp;

namespace Vertica.Integration.Globase.Ftp
{
	public class FtpUploader : IFtpUploader
	{
		private readonly IFtpClientFactory _ftpClientFactory;
	    private readonly IArchiveService _archive;
	    private readonly IGlobaseConfiguration _configuration;

	    public FtpUploader(IFtpClientFactory ftpClientFactory, IArchiveService archive, IKernel kernel)
		{
			_ftpClientFactory = ftpClientFactory;
			_archive = archive;
	        _configuration = kernel.Resolve<IGlobaseConfiguration>();
		}

		public string[] Upload(IEnumerable<FtpFile> files, Action<ArchiveCreated> onArchived = null)
		{
			if (files == null) throw new ArgumentNullException(nameof(files));

		    ArchiveOptions archiveOptions = _configuration.ArchiveOptions;

			if (archiveOptions != null)
			{
				var archive = _archive.Archive(archiveOptions.Name, newArchive =>
				{
					newArchive.Options.GroupedBy(archiveOptions.GroupName);

					if (archiveOptions.Expires.HasValue)
						newArchive.Options.ExpiresOn(archiveOptions.Expires.Value);

					foreach (FtpFile file in files)
						newArchive.IncludeContent(file.FileName, file.Contents);
				});

				onArchived?.Invoke(archive);
			}

			IFtpClient ftpClient = _ftpClientFactory.Create(_configuration.FtpConnectionString);

			var encoding = new UTF8Encoding(false /* to ensure that Globase can read the file, this parameter is necessary */);

			return files
				.Select(file => ftpClient.UploadFromString(file.FileName, file.Contents, encoding))
				.ToArray();
		}
	}
}
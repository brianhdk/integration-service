using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Infrastructure.Remote.Ftp;

namespace Vertica.Integration.Globase.Ftp
{
	public class FtpUploader : IFtpUploader
	{
		private readonly IFtpClientFactory _ftpClientFactory;
		private readonly GlobaseConfiguration _configuration;
		private readonly IArchiveService _archive;
		private readonly ArchiveOptions _archiveOptions;

		public FtpUploader(IFtpClientFactory ftpClientFactory, GlobaseConfiguration configuration, IArchiveService archive)
		{
			_ftpClientFactory = ftpClientFactory;
			_archive = archive;
			_configuration = configuration;
			_archiveOptions = configuration.ArchiveOptions;
		}

		public string[] Upload(IEnumerable<FtpFile> files, Action<ArchiveCreated> onArchived = null)
		{
			if (files == null) throw new ArgumentNullException(nameof(files));

			if (_archiveOptions != null)
			{
				var archive = _archive.Archive(_archiveOptions.Name, newArchive =>
				{
					newArchive.Options.GroupedBy(_archiveOptions.GroupName);

					if (_archiveOptions.Expires.HasValue)
						newArchive.Options.ExpiresOn(_archiveOptions.Expires.Value);

					foreach (FtpFile file in files)
						newArchive.IncludeContent(file.FileName, file.Contents);
				});

				onArchived?.Invoke(archive);
			}

			IFtpClient ftpClient = _ftpClientFactory.Create(_configuration.FtpConnectionStringInternal);

			var encoding = new UTF8Encoding(false /* to ensure that Globase can read the file, this parameter is necessary */);

			return files
				.Select(file => ftpClient.UploadFromString(file.FileName, file.Contents, encoding))
				.ToArray();
		}
	}
}
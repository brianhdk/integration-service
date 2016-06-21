using System;
using System.Collections.Generic;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Globase.Ftp
{
	public interface IFtpUploader
	{
		string[] Upload(IEnumerable<FtpFile> files, Action<ArchiveCreated> onArchived = null);
	}
}
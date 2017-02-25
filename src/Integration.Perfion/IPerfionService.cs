using System;
using System.Collections.Specialized;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Perfion
{
	public interface IPerfionService
	{
		PerfionXml Query(string query, Action<QueryArchiveOptions> archive = null);

		byte[] DownloadFile(Guid id);
		byte[] DownloadImage(Guid id, NameValueCollection options = null);
		byte[] DownloadPdfReport(int[] ids, string reportName, string language = null, NameValueCollection options = null);
	}

    public class QueryArchiveOptions : ArchiveOptions
    {
        public QueryArchiveOptions(string name)
            : base(name)
        {
        }

        public void Disable()
        {
            Disabled = true;
        }

        internal bool Disabled { get; private set; }
    }
}
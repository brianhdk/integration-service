using System;
using System.Collections.Specialized;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Perfion
{
    public interface IPerfionClient
	{
		PerfionXml Query(string query, Action<QueryArchiveOptions> archive = null);

		byte[] DownloadFile(Guid id);
		byte[] DownloadImage(Guid id, NameValueCollection options = null);
		byte[] DownloadPdfReport(int[] ids, string reportName, string language = null, NameValueCollection options = null);

        ConnectionString ConnectionString { get; }

        Uri BaseUri { get; }
        Uri WebServiceUri { get; }
	}
}
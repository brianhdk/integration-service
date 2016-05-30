using System;
using System.Collections.Specialized;

namespace Vertica.Integration.Perfion
{
	public interface IPerfionService
	{
		PerfionXml Query(string query);

		byte[] DownloadFile(Guid id);
		byte[] DownloadImage(Guid id, NameValueCollection options = null);
		byte[] DownloadPdfReport(int[] ids, string reportName, string language = null, NameValueCollection options = null);
	}
}
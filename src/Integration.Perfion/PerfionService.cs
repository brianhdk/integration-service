using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Xml.Linq;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Perfion.PerfionAPIService;

namespace Vertica.Integration.Perfion
{
	public class PerfionService : IPerfionService
	{
		private readonly IPerfionConfiguration _configuration;
		private readonly IArchiveService _archive;

		private readonly ArchiveOptions _archiveOptions;
		private readonly Uri _baseUri;

		public PerfionService(IPerfionConfiguration configuration, IArchiveService archive)
		{
			_configuration = configuration;
			_archive = archive;

			_baseUri = ParseBaseUri(configuration.ConnectionString);
			_archiveOptions = configuration.ArchiveOptions;
		}

		public PerfionXml Query(string query)
		{
			return Client(client =>
			{
				string xml = client.ExecuteQuery(query);

				ArchiveCreated archive = null;

				if (_archiveOptions != null)
				{
					archive = _archive.Archive(_archiveOptions.Name, newArchive =>
					{
						newArchive.Options.GroupedBy(_archiveOptions.GroupName);

						if (_archiveOptions.Expires.HasValue)
							newArchive.Options.ExpiresOn(_archiveOptions.Expires.Value);

						newArchive
							.IncludeContent("Query.txt", query)
							.IncludeContent("Data.txt", xml);
					});
				}

				return new PerfionXml(this, XDocument.Parse(xml), archive);
			});
		}

		public byte[] DownloadFile(Guid id)
		{
			return Download("File", id);
		}

		public byte[] DownloadImage(Guid id, NameValueCollection options = null)
		{
			return Download("Image", id, options);
		}

		private byte[] Download(string path, Guid id, NameValueCollection options = null)
		{
			using (var webClient = new WebClient())
			{
				string url = String.Format("{0}{1}.aspx?id={2}{3}",
					_baseUri,
					path,
					id,
					options != null
						? String.Join(String.Empty, options.AllKeys.Select(x => String.Format("&{0}={1}", x, options[x])))
						: String.Empty);

				return webClient.DownloadData(url);
			}
		}

		private static Uri ParseBaseUri(string webServiceUri)
		{
			Uri uri;
			if (!Uri.TryCreate(webServiceUri, UriKind.Absolute, out uri))
				throw new ArgumentException(String.Format("'{0}' is not a valid absolute uri.", webServiceUri));

			var builder = new UriBuilder(uri)
			{
				Path = String.Join(String.Empty, uri.Segments.Take(uri.Segments.Length - 1))
			};

			return builder.Uri;
		}

		private T Client<T>(Func<GetDataSoapClient, T> client)
		{
			if (client == null) throw new ArgumentNullException("client");

			var proxy = new GetDataSoapClient(new BasicHttpBinding
			{
				Name = "PerfionService",
				MaxReceivedMessageSize = Int32.MaxValue
			}, new EndpointAddress(_configuration.ConnectionString));

			try
			{
				return client(proxy);
			}
			finally
			{
				try
				{
					proxy.Close();
				}
				catch
				{
					proxy.Abort();
					throw;
				}
			}
		}
	}
}
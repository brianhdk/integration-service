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
		private readonly PerfionConfiguration _configuration;
		private readonly IArchiveService _archive;

		public PerfionService(PerfionConfiguration configuration, IArchiveService archive)
		{
			_configuration = configuration;
			_archive = archive;
		}

		public PerfionXml Query(string query)
		{
			return Client(client =>
			{
				string xml = client.ExecuteQuery(query);

				ArchiveCreated archive = null;

				ArchiveOptions archiveOptions = _configuration.ArchiveOptions;

				if (archiveOptions != null)
				{
					archive = _archive.Archive(archiveOptions.Name, newArchive =>
					{
						newArchive.Options.GroupedBy(archiveOptions.GroupName);

						if (archiveOptions.Expires.HasValue)
							newArchive.Options.ExpiresOn(archiveOptions.Expires.Value);

						newArchive
							.IncludeContent("Query.xml", query)
							.IncludeContent("Data.xml", xml);
					});
				}

				return new PerfionXml(this, XDocument.Parse(xml))
				{
					Archive = archive
				};
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
					ParseBaseUri(),
					path,
					id,
					options != null
						? String.Join(String.Empty, options.AllKeys.Select(x => String.Format("&{0}={1}", x, options[x])))
						: String.Empty);

				return webClient.DownloadData(url);
			}
		}

		private Uri ParseBaseUri()
		{
			string webServiceUri = _configuration.ConnectionStringInternal;

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

			var binding = new BasicHttpBinding
			{
				Name = "PerfionService",
				MaxReceivedMessageSize = _configuration.ServiceClientInternal.MaxReceivedMessageSize,
				ReceiveTimeout = _configuration.ServiceClientInternal.ReceiveTimeout,
				SendTimeout = _configuration.ServiceClientInternal.SendTimeout
			};

			if (_configuration.ServiceClientInternal.Binding != null)
				_configuration.ServiceClientInternal.Binding(binding);

			var proxy = new GetDataSoapClient(binding, new EndpointAddress(_configuration.ConnectionStringInternal));

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
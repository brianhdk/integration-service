using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Xml.Linq;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Perfion.PerfionAPIService;

namespace Vertica.Integration.Perfion
{
	public class PerfionService : IPerfionService
	{
		private readonly IPerfionConfiguration _configuration;
		private readonly IArchiveService _archive;
		private readonly IKernel _kernel;

		public PerfionService(IArchiveService archive, IKernel kernel)
		{
			_configuration = kernel.Resolve<IPerfionConfiguration>();
			_archive = archive;
			_kernel = kernel;
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
			return Download("File.aspx", id);
		}

		public byte[] DownloadImage(Guid id, NameValueCollection options = null)
		{
			return Download("Image.aspx", id, options);
		}

		public byte[] DownloadPdfReport(int[] ids, string reportName, string language = null, NameValueCollection options = null)
		{
			if (ids == null) throw new ArgumentNullException(nameof(ids));
			if (string.IsNullOrWhiteSpace(language)) throw new ArgumentException(@"Value cannot be null or empty", nameof(language));
			
			options = options ?? new NameValueCollection();

			if (!string.IsNullOrWhiteSpace(language))
				options.Set("lg", language);

			return Download($"../{reportName}.report", string.Join(",", ids.Distinct()), options);
		}

		private byte[] Download(string path, object id, NameValueCollection options = null)
		{
			using (var webClient = new WebClient())
			{
				string url =
					$"{ParseBaseUri()}{path}?id={id}{(options != null ? string.Join(string.Empty, options.AllKeys.Select(x => $"&{x}={options[x]}")) : string.Empty)}";

				_configuration.WebClientConfiguration.Configuration?.Invoke(_kernel, webClient);

				try
				{
					return webClient.DownloadData(url);
				}
				catch (Exception ex)
				{
					throw new WebException($"Error downloading from {url}. {ex.Message}.", ex);
				}
			}
		}

		private Uri ParseBaseUri()
		{
			string webServiceUri = _configuration.ConnectionString;

			Uri uri;
			if (!Uri.TryCreate(webServiceUri, UriKind.Absolute, out uri))
				throw new ArgumentException($"'{webServiceUri}' is not a valid absolute uri.");

			var builder = new UriBuilder(uri)
			{
				Path = string.Join(string.Empty, uri.Segments.Take(uri.Segments.Length - 1))
			};

			return builder.Uri;
		}

		private T Client<T>(Func<GetDataSoapClient, T> client)
		{
			if (client == null) throw new ArgumentNullException(nameof(client));

			var binding = new BasicHttpBinding
			{
				Name = "PerfionService",
				MaxReceivedMessageSize = _configuration.ServiceClientConfiguration.MaxReceivedMessageSize,
				ReceiveTimeout = _configuration.ServiceClientConfiguration.ReceiveTimeout,
				SendTimeout = _configuration.ServiceClientConfiguration.SendTimeout
			};

			_configuration.ServiceClientConfiguration.BindingInternal?.Invoke(_kernel, binding);

			var proxy = new GetDataSoapClient(binding, new EndpointAddress(_configuration.ConnectionString));

			_configuration.ServiceClientConfiguration.ClientCredentialsInternal?.Invoke(_kernel, proxy.ClientCredentials);

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
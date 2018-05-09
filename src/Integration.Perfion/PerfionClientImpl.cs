using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Perfion.Infrastructure.Client;
using Vertica.Integration.Perfion.PerfionAPIService;

namespace Vertica.Integration.Perfion
{
	internal class PerfionClientImpl : IPerfionClient
	{
	    private readonly Connection _connection;
	    private readonly IPerfionClientConfiguration _configuration;
	    private readonly IKernel _kernel;

		public PerfionClientImpl(Connection connection, IPerfionClientConfiguration configuration, IKernel kernel)
		{
		    if (connection == null) throw new ArgumentNullException(nameof(connection));
		    if (configuration == null) throw new ArgumentNullException(nameof(configuration));
		    if (kernel == null) throw new ArgumentNullException(nameof(kernel));

		    _connection = connection;
		    _configuration = configuration;
		    _kernel = kernel;
		}

		public PerfionXml Query(string query, Action<QueryArchiveOptions> archive = null)
		{
			return Client(client =>
			{
				string xml = client.ExecuteQuery(new ExecuteQueryRequest { Body = new ExecuteQueryRequestBody(query) }).Body.ExecuteQueryResult;

				ArchiveCreated archiveCreated = null;

                ArchiveOptions globalArchiveOptions = _configuration.ArchiveOptions;
                QueryArchiveOptions localArchiveOptions = GetLocalArchiveOptions(archive, globalArchiveOptions);

			    ArchiveOptions finalArchiveOptions = localArchiveOptions ?? globalArchiveOptions;

			    if (localArchiveOptions != null && localArchiveOptions.Disabled)
			        finalArchiveOptions = null;

				if (finalArchiveOptions != null)
				{
					archiveCreated = _kernel.Resolve<IArchiveService>().Archive(finalArchiveOptions.Name, newArchive =>
					{
						newArchive.Options.GroupedBy(finalArchiveOptions.GroupName);

						if (finalArchiveOptions.Expires.HasValue)
							newArchive.Options.ExpiresOn(finalArchiveOptions.Expires.Value);

						newArchive
							.IncludeContent("Query.xml", query)
							.IncludeContent("Data.xml", xml);
					});
				}

				return new PerfionXml(this, XDocument.Parse(xml))
				{
					Archive = archiveCreated
				};
			});
		}

	    private static QueryArchiveOptions GetLocalArchiveOptions(Action<QueryArchiveOptions> archive, ArchiveOptions archiveOptions)
	    {
	        if (archive == null)
	            return null;

	        var result = new QueryArchiveOptions(archiveOptions?.Name ?? "Data");

            result.GroupedBy(archiveOptions?.GroupName ?? "Perfion");

	        if (archiveOptions != null)
	        {
	            if (archiveOptions.Expires.HasValue)
	            {
	                result.ExpiresOn(archiveOptions.Expires.Value);
	            }
	        }
	        else
	        {
	            result.ExpiresAfterMonths(1);
	        }

	        archive(result);
	        return result;
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

	    public ConnectionString ConnectionString => _connection.ConnectionString;

	    public Uri BaseUri => _connection.GetBaseUri(_kernel);

	    public Uri WebServiceUri => _connection.GetWebServiceUri(_kernel);

	    private byte[] Download(string path, object id, NameValueCollection options = null)
		{
			using (WebClient webClient = _connection.CreateWebClient(_kernel))
			{
				string url = $"{path}?id={id}{(options != null ? string.Join(string.Empty, options.AllKeys.Select(x => $"&{x}={options[x]}")) : string.Empty)}";

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

		private T Client<T>(Func<GetDataSoap, T> client)
		{
			if (client == null) throw new ArgumentNullException(nameof(client));

		    return _connection.WithProxy(_kernel, client);
		}
	}
}
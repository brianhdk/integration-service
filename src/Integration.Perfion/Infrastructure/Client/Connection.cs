using System;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Perfion.PerfionAPIService;

namespace Vertica.Integration.Perfion.Infrastructure.Client
{
    public abstract class Connection
    {
        protected Connection(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        protected internal ConnectionString ConnectionString { get; }

        /// <summary>
        /// WebClient is used for all download related tasks, e.g. downloading files, images and reports.
        /// </summary>
        protected internal virtual WebClient CreateWebClient(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            return new WebClient
            {
                BaseAddress = GetBaseUri(kernel)?.ToString()
            };
        }

        protected internal virtual Uri GetBaseUri(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            Uri uri = GetWebServiceUri(kernel);

            var builder = new UriBuilder(uri)
            {
                Path = string.Join(string.Empty, uri.Segments.Take(uri.Segments.Length - 1))
            };

            return builder.Uri;
        }

        protected internal virtual Uri GetWebServiceUri(IKernel kernel)
        {
            Uri uri;
            if (!Uri.TryCreate(ConnectionString, UriKind.Absolute, out uri))
                throw new ArgumentException($"'{ConnectionString}' is not a valid absolute uri.");

            return uri;
        }

        internal virtual T WithProxy<T>(IKernel kernel, Func<GetDataSoap, T> client)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            var binding = new BasicHttpBinding
            {
                Name = $"PerfionService.{GetType().Name}",
            };

            ConfigureBinding(binding, kernel);
            
            GetDataSoapClient proxy = new GetDataSoapClient(binding, new EndpointAddress(GetWebServiceUri(kernel)));

            ConfigureClientCredentials(proxy.ClientCredentials, kernel);

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

        protected internal virtual void ConfigureClientCredentials(ClientCredentials clientCredentials, IKernel kernel)
        {
            if (clientCredentials == null) throw new ArgumentNullException(nameof(clientCredentials));
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));
        }

        protected internal virtual void ConfigureBinding(BasicHttpBinding binding, IKernel kernel)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.ReceiveTimeout = binding.SendTimeout = TimeSpan.MaxValue;
        }
    }
}
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
            if (!Uri.TryCreate(ConnectionString, UriKind.Absolute, out Uri uri))
                throw new ArgumentException($"'{ConnectionString}' is not a valid absolute uri.");

            return uri;
        }

        internal virtual T WithProxy<T>(IKernel kernel, Func<GetDataSoap, T> client)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            Uri uri = GetWebServiceUri(kernel);

            HttpBindingBase binding = CreateBinding($"PerfionService.{GetType().Name}", uri, kernel);

            ConfigureBinding(binding, kernel);
            
            GetDataSoapClient proxy = new GetDataSoapClient(binding, new EndpointAddress(uri));

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

        protected internal virtual HttpBindingBase CreateBinding(string name, Uri uri, IKernel kernel)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty", nameof(name));
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (!uri.IsAbsoluteUri) throw new ArgumentOutOfRangeException($"'{uri}' is not a valid absolute uri.");
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            if (Uri.UriSchemeHttps.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
                return new BasicHttpsBinding { Name = name };

            return new BasicHttpBinding { Name = name };
        }

        protected internal virtual void ConfigureClientCredentials(ClientCredentials clientCredentials, IKernel kernel)
        {
            if (clientCredentials == null) throw new ArgumentNullException(nameof(clientCredentials));
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));
        }

        protected internal virtual void ConfigureBinding(HttpBindingBase binding, IKernel kernel)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.ReceiveTimeout = binding.SendTimeout = TimeSpan.MaxValue;
        }

        [Obsolete("Use ConfigureBinding(HttpBindingBase binding, IKernel kernel) method instead.")]
        protected internal virtual void ConfigureBinding(BasicHttpBinding binding, IKernel kernel)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            ConfigureBinding((HttpBindingBase) binding, kernel);
        }
    }
}
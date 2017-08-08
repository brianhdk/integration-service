﻿using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Perfion.Infrastructure.Client
{
    public sealed class DefaultConnection : Connection
    {
        private readonly Connection _connection;

        internal DefaultConnection(ConnectionString connectionString)
            : base(connectionString)
        {
        }

        internal DefaultConnection(Connection connection)
            : base(connection.ConnectionString)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _connection = connection;
        }

        protected internal override WebClient CreateWebClient(IKernel kernel)
        {
            if (_connection != null)
                return _connection.CreateWebClient(kernel);

            return base.CreateWebClient(kernel);
        }

        protected internal override Uri GetBaseUri(IKernel kernel)
        {
            if (_connection != null)
                return _connection.GetBaseUri(kernel);

            return base.GetBaseUri(kernel);
        }

        protected internal override Uri GetWebServiceUri(IKernel kernel)
        {
            if (_connection != null)
                return _connection.GetWebServiceUri(kernel);

            return base.GetWebServiceUri(kernel);
        }

        protected internal override void ConfigureClientCredentials(ClientCredentials clientCredentials, IKernel kernel)
        {
            if (_connection != null)
            {
                _connection.ConfigureClientCredentials(clientCredentials, kernel);
                return;
            }

            base.ConfigureClientCredentials(clientCredentials, kernel);
        }

        protected internal override void ConfigureBinding(BasicHttpBinding binding, IKernel kernel)
        {
            if (_connection != null)
            {
                _connection.ConfigureBinding(binding, kernel);
                return;
            }

            base.ConfigureBinding(binding, kernel);
        }
    }
}
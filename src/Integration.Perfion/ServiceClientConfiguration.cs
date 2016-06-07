using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Vertica.Integration.Perfion
{
	public class ServiceClientConfiguration
	{
		public ServiceClientConfiguration()
		{
			MaxReceivedMessageSize = int.MaxValue;
			ReceiveTimeout = SendTimeout = TimeSpan.MaxValue;
		}

		public long MaxReceivedMessageSize { get; set; }
		public TimeSpan ReceiveTimeout { get; set; }
		public TimeSpan SendTimeout { get; set; }

		internal Action<BasicHttpBinding> Binding { get; private set; }
		internal Action<ClientCredentials> ClientCredentials { get; private set; }

		public ServiceClientConfiguration Advanced(Action<BasicHttpBinding> binding = null, Action<ClientCredentials> clientCredentials = null)
		{
			Binding = binding;
			ClientCredentials = clientCredentials;

			return this;
		}

		public ServiceClientConfiguration Change(Action<ServiceClientConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}
	}
}
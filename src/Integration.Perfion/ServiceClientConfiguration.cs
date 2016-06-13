using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using Castle.MicroKernel;

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

		internal Action<IKernel, BasicHttpBinding> Binding { get; private set; }
		internal Action<IKernel, ClientCredentials> ClientCredentials { get; private set; }
		internal Action<IKernel, WebClient> WebClient { get; private set; }

		public ServiceClientConfiguration Advanced(
			Action<IKernel, BasicHttpBinding> binding = null, 
			Action<IKernel, ClientCredentials> clientCredentials = null,
			Action<IKernel, WebClient> webClient = null)
		{
			Binding = binding;
			ClientCredentials = clientCredentials;
			WebClient = webClient;

			return this;
		}

		public ServiceClientConfiguration Change(Action<ServiceClientConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}
	}
}
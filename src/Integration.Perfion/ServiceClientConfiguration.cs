using System;
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

		internal Action<IKernel, BasicHttpBinding> BindingInternal { get; private set; }
		internal Action<IKernel, ClientCredentials> ClientCredentialsInternal { get; private set; }

		public ServiceClientConfiguration Binding(Action<IKernel, BasicHttpBinding> binding)
		{
			if (binding == null) throw new ArgumentNullException(nameof(binding));

			BindingInternal = binding;

			return this;
		}

		public ServiceClientConfiguration ClientCredentials(Action<IKernel, ClientCredentials> clientCredentials)
		{
			if (clientCredentials == null) throw new ArgumentNullException(nameof(clientCredentials));

			ClientCredentialsInternal = clientCredentials;

			return this;
		}

		[Obsolete("This method throws. Use the specific Binding and ClientCredentials methods instead.")]
		public ServiceClientConfiguration Advanced(Action<BasicHttpBinding> binding = null, Action<ClientCredentials> clientCredentials = null)
		{
			throw new NotSupportedException("Use the specific methods. Binding(...) and ClientCredentials(...)");
		}

		public ServiceClientConfiguration Change(Action<ServiceClientConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}
	}
}
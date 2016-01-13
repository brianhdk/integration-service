using System;
using System.ServiceModel;

namespace Vertica.Integration.Perfion
{
	public class ServiceClientConfiguration
	{
		public ServiceClientConfiguration()
		{
			MaxReceivedMessageSize = Int32.MaxValue;
			ReceiveTimeout = SendTimeout = TimeSpan.MaxValue;
		}

		public long MaxReceivedMessageSize { get; set; }
		public TimeSpan ReceiveTimeout { get; set; }
		public TimeSpan SendTimeout { get; set; }

		internal Action<BasicHttpBinding> Binding { get; private set; }

		public ServiceClientConfiguration Advanced(Action<BasicHttpBinding> binding)
		{
			if (binding == null) throw new ArgumentNullException("binding");

			Binding = binding;
			return this;
		}

		public ServiceClientConfiguration Change(Action<ServiceClientConfiguration> change)
		{
			if (change != null)
				change(this);

			return this;
		}
	}
}
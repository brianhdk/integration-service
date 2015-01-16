using System;

namespace Vertica.Integration.Infrastructure.Configuration
{
	public class Parameters
	{
		public Parameters()
		{
			SetDefaults();
		}

		public virtual int Id { get; protected set; }

		public virtual DateTimeOffset LastMonitorCheck { get; set; }

		private void SetDefaults()
		{
			Id = 1;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Reflection;
using Vertica.Integration.Infrastructure.Database.NHibernate.Mappings.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Database.NHibernate.Connections
{
	public class IntegrationDb : DefaultConnection
	{
		public IntegrationDb()
			: base("IntegrationDb")
		{
		}

		protected override IEnumerable<Assembly> MappingAssemblies
		{
			get { yield return typeof(ErrorLogMap).Assembly; }
		}

		protected override IEnumerable<Type> MappingTypes
		{
			get { yield break; }
		}
	}
}
using FluentNHibernate.Mapping;
using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration.Infrastructure.Database.NHibernate.Mappings.Infrastructure.Configuration
{
	public class ParametersMap : ClassMap<Parameters>
	{
		public ParametersMap()
		{
			Id(x => x.Id).GeneratedBy.Assigned();

			Map(x => x.LastMonitorCheck);
			Map(x => x.LastCatalogImport);
		}
	}
}
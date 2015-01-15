using FluentNHibernate.Mapping;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Database.NHibernate.Mappings.Infrastructure.Logging
{
    public class ErrorLogMap : ClassMap<ErrorLog>
    {
        public ErrorLogMap()
        {
            Id(x => x.Id);
			Map(x => x.MachineName);
			Map(x => x.IdentityName);
	        Map(x => x.CommandLine);
            Map(x => x.Severity);
            Map(x => x.Message).CustomSqlType("nvarchar(max)");
			// http://ayende.com/blog/1969/nhibernate-and-large-text-fields-gotchas
            Map(x => x.FormattedMessage).CustomType("StringClob");
            Map(x => x.TimeStamp);
            Map(x => x.Target);
        }
    }
}
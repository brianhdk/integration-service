using FluentNHibernate.Mapping;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Database.NHibernate.Mappings.Infrastructure.Logging
{
    public class TaskLogMap : SubclassMap<TaskLog>
    {
        public TaskLogMap()
        {
			HasMany(x => x.Messages)
				.Access.CamelCaseField(Prefix.Underscore)
				.Cascade.AllDeleteOrphan()
				.KeyColumn("TaskLog_Id");

			HasMany(x => x.Steps)
				.Access.CamelCaseField(Prefix.Underscore)
				.Cascade.AllDeleteOrphan()
				.KeyColumn("TaskLog_Id");

            References(x => x.ErrorLog);
            
            DiscriminatorValue("T");
        }
    }
}
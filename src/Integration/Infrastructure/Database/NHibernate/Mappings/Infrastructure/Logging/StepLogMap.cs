using FluentNHibernate.Mapping;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Database.NHibernate.Mappings.Infrastructure.Logging
{
    public class StepLogMap : SubclassMap<StepLog>
    {
        public StepLogMap()
        {
	        References(x => x.TaskLog).Column("TaskLog_Id");

            Map(x => x.StepName);

	        HasMany(x => x.Messages)
				.Access.CamelCaseField(Prefix.Underscore)
				.Cascade.AllDeleteOrphan()
				.KeyColumn("StepLog_Id");

			References(x => x.ErrorLog);

            DiscriminatorValue("S");
        }
    }
}
using FluentNHibernate.Mapping;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Database.NHibernate.Mappings.Infrastructure.Logging
{
    public class LogEntryMap : ClassMap<LogEntry>
    {
         public LogEntryMap()
         {
             Table("TaskLog");

             Id(x => x.Id);
             Map(x => x.TaskName);
	         Map(x => x.ExecutionTimeSeconds);
	         Map(x => x.TimeStamp);

             DiscriminateSubClassesOnColumn("Type");
         }
    }
}
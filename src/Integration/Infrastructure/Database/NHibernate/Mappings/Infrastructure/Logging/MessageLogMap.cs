using FluentNHibernate.Mapping;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Infrastructure.Database.NHibernate.Mappings.Infrastructure.Logging
{
    public class MessageLogMap : SubclassMap<MessageLog>
    {
         public MessageLogMap()
         {
			 References(x => x.TaskLog).Column("TaskLog_Id");
			 References(x => x.StepLog).Column("StepLog_Id");

			 Map(x => x.StepName);
             Map(x => x.Message).CustomSqlType("nvarchar(max)");

             DiscriminatorValue("M");
         }
    }
}
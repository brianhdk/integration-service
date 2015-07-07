using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Experiments.Monitoring;

namespace Vertica.Integration.Experiments
{
    public static class MonitorTaskTester
    {
        public static ApplicationConfiguration TestMonitorTask(this ApplicationConfiguration application)
        {
            return application
                .Tasks(tasks => tasks
                    .MonitorTask(task => task
                        .Clear()
                        .Step<MonitorFoldersStep>()));
        }
    }
}
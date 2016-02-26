using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Model;

namespace Vertica.Integration.Logging.Elmah
{
    public static class ElmahExtensions
    {
        public static TaskConfiguration<MonitorWorkItem> IncludeElmah(this TaskConfiguration<MonitorWorkItem> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            return task.Step<ExportElmahErrorsStep>();
        }

        public static TaskConfiguration<MaintenanceWorkItem> IncludeElmah(this TaskConfiguration<MaintenanceWorkItem> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            return task.Step<CleanUpElmahErrorsStep>();
        }
    }
}
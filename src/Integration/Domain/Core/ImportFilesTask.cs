using System.Collections.Generic;
using System.Threading;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
	public class ImportFilesTask : Task<ImportFilesWorkItem>
	{
		public ImportFilesTask(IEnumerable<IStep<ImportFilesWorkItem>> steps) : base(steps)
		{
		}

		public override string Description
		{
			get { return "Imports various files to the portal."; }
		}

		public override string Schedule
		{
			get { return "Every hour."; }
		}

		public override ImportFilesWorkItem Start(Log log, params string[] arguments)
		{
            // we do this to have as little cpu-utilization as possible while importing files
		    Thread.CurrentThread.Priority = ThreadPriority.Lowest;

			return new ImportFilesWorkItem();
		}
	}
}
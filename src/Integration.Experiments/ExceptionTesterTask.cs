using System;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class ExceptionTesterTask : Task
    {
        public override string Description
        {
            get { return "TBD"; }
        }

        public override void StartTask(ILog log, params string[] arguments)
        {
            throw new InvalidOperationException("Some exception.");
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Vertica.Integration.Infrastructure.Parsing;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Infrastructure.Remote.Ftp;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class FtpTesterTask : Task
    {
        private readonly IFtpClientFactory _factory;
	    private readonly ICsvParser _csvParser;
	    private readonly ITaskRunner _taskRunner;
	    private readonly ITaskFactory _taskFactory;

        public FtpTesterTask(IFtpClientFactory factory, ICsvParser csvParser, ITaskRunner taskRunner, ITaskFactory taskFactory)
        {
	        _factory = factory;
	        _csvParser = csvParser;
	        _taskRunner = taskRunner;
	        _taskFactory = taskFactory;
        }

	    public override void StartTask(ITaskExecutionContext context)
	    {
		    _taskRunner.Execute(_taskFactory.Get<SomeTask>());
			//IFtpClient client = _factory.Create("ftp://ftp.vertica.dk/BHK", ftp => ftp
			//	.Credentials("GuestFTP", "VerticaPass1010"));

			//IFtpClient client = _factory.Create(new Uri("ftp://ftp.globase.com:2121"), ftp => ftp
			//	.Credentials("pjustesen_demo", "gaePh3ee"));

		    IFtpClient client = _factory.Create("ftp://pjustesen_demo:gaePh3ee@ftp.globase.com:2121");
			//client.UploadFromString("/Test.txt", "test");

			context.Log.Message(String.Join(Environment.NewLine, client.ListDirectory()));
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }

	public class SomeTask : Task
	{
		public override string Description
		{
			get { return "TBD"; }
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			context.Log.Message("OK!");
		}
	}
}
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

        public FtpTesterTask(IFtpClientFactory factory, ICsvParser csvParser)
        {
	        _factory = factory;
	        _csvParser = csvParser;
        }

	    public override void StartTask(ITaskExecutionContext context)
        {
            IFtpClient client = _factory.Create("ftp://ftp.vertica.dk/BHK", ftp => ftp
                .Credentials("GuestFTP", "VerticaPass1010"));
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }
}
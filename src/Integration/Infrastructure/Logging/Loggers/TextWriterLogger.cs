using System.IO;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
	internal class TextWriterLogger : Logger
    {
		private readonly TextWriter _textWriter;
		private readonly TextWriterLoggerConfiguration _configuration;

		public TextWriterLogger(TextWriter textWriter, TextWriterLoggerConfiguration configuration)
		{
			_textWriter = textWriter;
			_configuration = configuration;
		}

		protected override string Insert(TaskLog log)
		{
			// Text writer will not do anything, as TextWriter is already used by TaskRunner
			return null;
		}

        protected override string Insert(StepLog log)
        {
			// Text writer will not do anything, as TextWriter is already used by TaskRunner
	        return null;
        }

        protected override string Insert(MessageLog log)
        {
			// Text writer will not do anything, as TextWriter is already used by TaskRunner
			return null;
        }

        protected override string Insert(ErrorLog log)
        {
	        _configuration.Write(_textWriter, log);

	        return null;
        }

        protected override void Update(TaskLog log)
        {
        }

        protected override void Update(StepLog log)
        {
        }
    }
}
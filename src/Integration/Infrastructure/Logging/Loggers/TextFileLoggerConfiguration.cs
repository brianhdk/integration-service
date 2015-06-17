using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public class TextFileLoggerConfiguration
    {
        public TextFileLoggerConfiguration Location(DirectoryInfo location)
        {
            if (location == null) throw new ArgumentNullException("location");

            return this;
        }
    }
}
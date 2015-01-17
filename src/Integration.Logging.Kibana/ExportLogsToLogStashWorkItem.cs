using System;
using System.IO;

namespace Vertica.Integration.Logging.Kibana
{
    public class ExportLogsToLogStashWorkItem
    {
        private readonly Action<Stream, string> _upload;

        public ExportLogsToLogStashWorkItem(Action<Stream, string> upload)
        {
            if (upload == null) throw new ArgumentNullException("upload");

            _upload = upload;
        }

        public void Upload(Stream stream, string name)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.");

            _upload(stream, name);
        }
    }
}
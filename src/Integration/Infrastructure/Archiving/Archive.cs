using System;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class Archive
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long ByteSize { get; set; }
        public DateTimeOffset Created { get; set; }
        public string GroupName { get; set; }
        public DateTimeOffset? Expires { get; set; }
    }
}
using System;

namespace Vertica.Integration.Infrastructure.Configuration
{
    public class Configuration
    {
        public string ClrType { get; set; }
        public string JsonData { get; set; }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string UpdatedBy { get; set; }
    }
}
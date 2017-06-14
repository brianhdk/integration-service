using System;
using Newtonsoft.Json;
using Vertica.Utilities;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class ArchiveOptions
    {
	    public ArchiveOptions(string name)
        {
            Named(name);
        }

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public string GroupName { get; private set; }

        [JsonProperty]
        public virtual DateTimeOffset? Expires { get; private set; }

        public ArchiveOptions Named(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

            Name = name;
            return this;
        }

        public ArchiveOptions GroupedBy(string name)
        {
            GroupName = name;
            return this;
        }

        public ArchiveOptions ExpiresOn(DateTimeOffset dateTime)
        {
            Expires = dateTime;
            return this;
        }

        public virtual ArchiveOptions ExpiresAfter(TimeSpan timeSpan)
        {
            return ExpiresOn(Time.UtcNow.Add(timeSpan));
        }

        public ArchiveOptions ExpiresAfterDays(uint days)
        {
            return ExpiresAfter(TimeSpan.FromDays(days));
        }

        public virtual ArchiveOptions ExpiresAfterMonths(uint months)
        {
            return ExpiresOn(Time.UtcNow.AddMonths((int)months));
        }
    }
}
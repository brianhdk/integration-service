using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vertica.Utilities.Extensions.StringExt;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat.Logging
{
    public class HeartbeatLogEntry : IComparable<HeartbeatLogEntry>
    {
        private readonly List<Group> _groups;

        public HeartbeatLogEntry(DateTimeOffset created, params Group[] groups)
        {
            Created = created;
            _groups = groups?.ToList() ?? new List<Group>();
        }

        public DateTimeOffset Created { get; }
        public Group[] Groups => _groups.ToArray();

        internal void CollectFrom(IHeartbeatProvider provider, CancellationToken token)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            string name = provider.GetType().Name;
            name = name.Strip("HeartbeatProvider").NullIfEmpty() ?? name;

            string[] messages = provider.CollectHeartbeatMessages(token)?.ToArray();

            _groups.Add(new Group(name, messages ?? new string[0]));
        }

        public class Group
        {
            public Group(string name, params string[] messages)
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty", nameof(name));
                
                Name = name;
                Messages = messages ?? new string[0];
            }

            public string Name { get; }
            public string[] Messages { get; }
        }

        public int CompareTo(HeartbeatLogEntry other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            return other.Created.CompareTo(Created);
        }
    }
}
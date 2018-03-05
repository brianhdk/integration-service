using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Vertica.Utilities;
using Vertica.Utilities.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat.Logging
{
    [Guid("58E02939-629E-4A6D-903F-AB4DF87CBEEA")]
    [Description("Heartbeat Log from the LiteServer functionality.")]
    internal class HeartbeatLog
    {
        private Dictionary<string, Entries> _byMachineName;

        public Dictionary<string, Entries> ByMachineName
        {
            get => _byMachineName ?? (_byMachineName = new Dictionary<string, Entries>());
            set => _byMachineName = value ?? new Dictionary<string, Entries>();
        }

        public void Add(string machineName, HeartbeatLogEntry entry)
        {
            if (string.IsNullOrWhiteSpace(machineName)) throw new ArgumentException(@"Value cannot be null or empty", nameof(machineName));
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            if (!ByMachineName.TryGetValue(machineName, out Entries entries))
                ByMachineName.Add(machineName, entries = new Entries());

            entries.Add(entry);
        }

        public void Cleanup(int maximumEntries, TimeSpan maximumAge)
        {
            var toBeRemoved = new List<string>();

            foreach (KeyValuePair<string, Entries> entries in ByMachineName)
            {
                entries.Value.Cleanup(maximumEntries, maximumAge);

                if (entries.Value.Count == 0)
                    toBeRemoved.Add(entries.Key);
            }

            toBeRemoved.ForEach(x => ByMachineName.Remove(x));
        }

        public class Entries : IEnumerable<HeartbeatLogEntry>
        {
            private readonly List<HeartbeatLogEntry> _list;

            public Entries(IEnumerable<HeartbeatLogEntry> list = null)
            {
                _list = list.EmptyIfNull().ToList();
                _list.Sort();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<HeartbeatLogEntry> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            public int Count => _list.Count;

            public void Add(HeartbeatLogEntry entry)
            {
                if (entry == null) throw new ArgumentNullException(nameof(entry));

                _list.Insert(0, entry);
            }

            public void Cleanup(int maximumEntries, TimeSpan maximumAge)
            {
                if (_list.Count > maximumEntries)
                    _list.RemoveRange(maximumEntries, _list.Count - maximumEntries);

                var now = Time.UtcNow;

                _list.RemoveAll(x => now - x.Created >= maximumAge);
            }
        }
    }
}
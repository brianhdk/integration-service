using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat.Logging
{
    [Guid("58E02939-629E-4A6D-903F-AB4DF87CBEEA")]
    [Description("Heartbeat Log from the LiteServer functionality.")]
    internal class HeartbeatLog
    {
        private List<HeartbeatLogEntry> _entries;

        public HeartbeatLog()
        {
            _entries = new List<HeartbeatLogEntry>();
        }

        public List<HeartbeatLogEntry> Entries
        {
            get => _entries;
            set => _entries = value ?? new List<HeartbeatLogEntry>();
        }
    }
}
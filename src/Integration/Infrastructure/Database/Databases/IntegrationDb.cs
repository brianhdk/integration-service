using System;

namespace Vertica.Integration.Infrastructure.Database.Databases
{
    internal class IntegrationDb : DefaultConnection
    {
        public IntegrationDb(ConnectionString connectionString)
            : base(connectionString)
        {
        }

        public static DefaultConnection Disabled
        {
            get { return new DisabledConnection(); }
        }

        private class DisabledConnection : DefaultConnection, IDisabledConnection
        {
            public DisabledConnection() : base(ConnectionString.FromText(String.Empty))
            {
            }

            public string ExceptionMessage
            {
                get { return "IntegrationDb has been disabled."; }
            }
        }
    }
}
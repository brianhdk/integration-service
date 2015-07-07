﻿using System;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace Vertica.Integration.Infrastructure.Database.Databases
{
    [Serializable]
    public class IntegrationDbException : Exception
    {
        public IntegrationDbException()
        {
        }

        public IntegrationDbException(SqlException inner)
            : base(String.Format(@"{0}

Make sure the database is created, that it is functional and up-to-date with the latest migrations. 

Try run the ""MigrateTask"" to ensure it's running with the latest database schema.

If this error continues, check your connection string and permissions to the database.", inner.Message), inner)
        {
        }

        protected IntegrationDbException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
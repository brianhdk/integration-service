﻿using System;
using System.Configuration;

namespace Vertica.Integration.Infrastructure
{
    public sealed class ConnectionString
    {
        private readonly Lazy<string> _value;

        private ConnectionString(Func<string> value)
        {
            _value = new Lazy<string>(value);
        }   

        public static ConnectionString FromName(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            return new ConnectionString(() =>
            {
                ConnectionStringSettings connectionString =
                    ConfigurationManager.ConnectionStrings[name];

                if (connectionString == null)
                    throw new ArgumentException(
                        String.Format("No ConnectionString found with name '{0}'. Please add this to the <connectionString> element.", name));

                return connectionString.ConnectionString;
            });
        }

        public static ConnectionString FromText(string text)
        {
            return new ConnectionString(() => text);
        }

        public override string ToString()
        {
            return _value.Value;
        }

        public static implicit operator string(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            return connectionString.ToString();
        }
    }
}
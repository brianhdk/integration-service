using System;
using System.Configuration;

namespace Vertica.Integration.Infrastructure
{
    public sealed class ConnectionString : IEquatable<ConnectionString>
    {
	    private readonly Lazy<string> _value;

        private ConnectionString(Func<string> value)
        {
            _value = new Lazy<string>(value);
        }   

        public static ConnectionString FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

            return new ConnectionString(() =>
            {
                ConnectionStringSettings connectionString =
                    ConfigurationManager.ConnectionStrings[name];

                if (connectionString == null)
                    throw new ArgumentException(
	                    $"No ConnectionString found with name '{name}'. Please add this to the <connectionString> element.");

                return connectionString.ConnectionString;
            });
        }

        public static ConnectionString FromText(string text)
        {
            return new ConnectionString(() => text);
        }

	    public static ConnectionString Empty => FromText(string.Empty);

	    public override string ToString()
        {
            return _value.Value ?? string.Empty;
        }

		public bool Equals(ConnectionString other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ConnectionString && Equals((ConnectionString)obj);
		}

		public override int GetHashCode()
		{
			return ToString().ToLowerInvariant().GetHashCode();
		}

        public static implicit operator string(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            return connectionString.ToString();
        }
    }
}
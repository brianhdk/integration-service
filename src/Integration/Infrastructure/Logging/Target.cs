using System;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class Target : IEquatable<Target>
    {
        private readonly string _name;

        protected Target(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public override string ToString()
        {
            return _name;
        }

        public static implicit operator string(Target target)
        {
            if (target == null)
                return null;

            return target.ToString();
        }

        public bool Equals(Target other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return String.Equals(_name, other._name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Target) obj);
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public static readonly Target Service = new Target("Service");
        public static readonly Target All = new Target("All");

        public static Target Custom(string name)
        {
            Target custom = new Target(name);

            if (custom.Equals(Service) || custom.Equals(All))
                throw new ArgumentException(String.Format("'{0}' is a reserved name.", name));

            return custom;
        }
    }
}
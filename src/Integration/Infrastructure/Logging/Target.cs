using System;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class Target : ITarget, IEquatable<Target>
    {
        protected Target(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }

        public static implicit operator string(Target target)
        {
            if (target == null)
                return null;

            return target.ToString();
        }

        public static implicit operator Target(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return Custom(name);
        }

        public bool Equals(Target other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is Target)) return false;
            return Equals((Target) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static readonly Target Service = new Target("Service");
        public static readonly Target All = new Target("All");

        public static Target Custom(string name)
        {
            var custom = new Target(name);

            if (custom.Equals(Service))
                return Service;

            if (custom.Equals(All))
                return All;

            return custom;
        }
    }
}
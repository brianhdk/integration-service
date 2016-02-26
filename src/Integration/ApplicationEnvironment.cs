using System;

namespace Vertica.Integration
{
	public class ApplicationEnvironment : IEquatable<ApplicationEnvironment>
	{
		protected ApplicationEnvironment(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

			Name = name;
		}

		public string Name { get; private set; }

		public override string ToString()
		{
			return Name;
		}

		public static implicit operator string(ApplicationEnvironment target)
		{
			if (target == null)
				return null;

			return target.ToString();
		}

		public static implicit operator ApplicationEnvironment(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return null;

			return Custom(name);
		}

		public static bool operator ==(ApplicationEnvironment a, ApplicationEnvironment b)
		{
			return Equals(a, b);
		}

		public static bool operator !=(ApplicationEnvironment a, ApplicationEnvironment b)
		{
			return !Equals(a, b);
		}

		public bool Equals(ApplicationEnvironment other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (!(obj is ApplicationEnvironment)) return false;
			return Equals((ApplicationEnvironment)obj);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public static readonly ApplicationEnvironment Development = new ApplicationEnvironment("Development");
		public static readonly ApplicationEnvironment Stage = new ApplicationEnvironment("Stage");
		public static readonly ApplicationEnvironment Production = new ApplicationEnvironment("Production");

		[Obsolete("Renamed to Stage.")]
		public static readonly ApplicationEnvironment Staging = new ApplicationEnvironment("Staging");

		public static ApplicationEnvironment Custom(string name)
		{
			var custom = new ApplicationEnvironment(name);

			if (custom.Equals(Development))
				return Development;

			if (custom.Equals(Stage))
				return Stage;

#pragma warning disable 618
			if (custom.Equals(Staging))
				return Staging;
#pragma warning restore 618

			if (custom.Equals(Production))
				return Production;

			return custom;
		}
	}
}
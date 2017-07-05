using System;
using Vertica.Utilities;
using Vertica.Utilities.Extensions.StringExt;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class HandleAsWindowsService
	{
		public HandleAsWindowsService(string name, string displayName, string description, Func<IDisposable> onStartFactory = null)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));
			if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(displayName));

			Name = name;
			DisplayName = displayName;
			Description = description.NullIfEmpty();
			OnStartFactory = onStartFactory ?? (() => new DisposableAction(() => { }));
		}

		public string Name { get; }
		public string DisplayName { get; }
		public string Description { get; }

		internal Func<IDisposable> OnStartFactory { get; }
	}
}
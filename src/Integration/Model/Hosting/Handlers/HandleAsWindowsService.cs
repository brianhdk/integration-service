using System;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class HandleAsWindowsService
	{
		public HandleAsWindowsService(string name, string displayName, string description, Func<IDisposable> onStartFactory = null)
		{
			if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");
			if (String.IsNullOrWhiteSpace(displayName)) throw new ArgumentException(@"Value cannot be null or empty.", "displayName");

			Name = name;
			DisplayName = displayName;
			Description = description.NullIfEmpty();
			OnStartFactory = onStartFactory ?? (() => new DisposableAction(() => { }));
		}

		public string Name { get; private set; }
		public string DisplayName { get; private set; }
		public string Description { get; private set; }

		internal Func<IDisposable> OnStartFactory { get; private set; }
	}
}
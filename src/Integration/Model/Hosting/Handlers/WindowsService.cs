using System;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class WindowsService
	{
		public WindowsService(string name, string description)
		{
			if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");
			if (String.IsNullOrWhiteSpace(description)) throw new ArgumentException(@"Value cannot be null or empty.", "description");

			Name = name;
			Description = description;
			OnStartFactory = () => new DisposableAction(() => { });
		}

		public string Name { get; private set; }
		public string Description { get; private set; }

		internal Func<IDisposable> OnStartFactory { get; private set; }

		public WindowsService OnStart(Func<IDisposable> action)
		{
			if (action == null) throw new ArgumentNullException("action");

			OnStartFactory = action;

			return this;
		}
	}
}
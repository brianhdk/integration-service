using System;
using System.IO;
using System.Threading;
using Vertica.Integration.Infrastructure.Threading;

namespace Vertica.Integration.Infrastructure.Extensions
{
	public static class ActionRepeaterExtensions
    {
		public static ActionRepeater Repeat(this TimeSpan delay, Action action, CancellationToken cancellationToken, TextWriter outputter = null)
		{
			return action.Repeat(delay, cancellationToken, outputter);
		}

        public static ActionRepeater Repeat(this Action action, TimeSpan delay, CancellationToken cancellationToken, TextWriter outputter = null)
        {
            return ActionRepeater.Start(action, delay, cancellationToken, outputter);
        }
    }
}
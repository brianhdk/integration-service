using System.Linq;
using Vertica.Utilities_v4.Extensions.EnumerableExt;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MessageContainsTextIgnoreFilter : PredicateSpecification<MonitorEntry>
	{
		public MessageContainsTextIgnoreFilter(params string[] texts)
			: base(entry => texts.EmptyIfNull().Any(text => entry.Message.Contains(text)))
		{
		}
	}
}
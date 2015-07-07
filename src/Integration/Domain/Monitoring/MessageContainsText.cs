using System;
using System.Linq;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Domain.Monitoring
{
	public class MessageContainsText : Specification<MonitorEntry>
	{
	    private readonly string[] _texts;

	    public MessageContainsText(params string[] texts)
	    {
	        _texts = texts ?? new string[0];
	    }

	    public override bool IsSatisfiedBy(MonitorEntry context)
	    {
	        return _texts.Any(text => 
                context.Message != null && 
                context.Message.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
	    }
	}
}
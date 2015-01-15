using System;
using System.Collections.Generic;
using System.Linq;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.EnumerableExt;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Domain.Monitoring
{
    public class MonitorWorkItem
    {
	    private readonly Range<DateTimeOffset> _checkRange;
	    private readonly List<Tuple<Target, MonitorEntry>> _entries;
	    private readonly ISpecification<MonitorEntry>[] _ignoreFilters;

	    public MonitorWorkItem(DateTimeOffset lastErrorCheck, params ISpecification<MonitorEntry>[] ignoreFilters)
        {
		    var upperBound = Time.UtcNow;

		    if (lastErrorCheck > upperBound)
                upperBound = lastErrorCheck;

		    _checkRange = new Range<DateTimeOffset>(lastErrorCheck, upperBound);
		    _entries = new List<Tuple<Target, MonitorEntry>>();
		    _ignoreFilters = ignoreFilters.EmptyIfNull().SkipNulls().ToArray();
        }

		public Range<DateTimeOffset> CheckRange
		{
			get { return _checkRange; }
		}

        public void Add(MonitorEntry entry, Target target)
        {
            if (entry == null) throw new ArgumentNullException("entry");

            if (!_ignoreFilters.Any(filter => filter.IsSatisfiedBy(entry)))
                _entries.Add(Tuple.Create(target, entry));
        }

        public void Add(DateTimeOffset dateTime, string source, string message, Target target)
        {
            Add(new MonitorEntry(dateTime, source, message), target);
        }

        public MonitorEntry[] GetEntries(Target target)
        {
            return _entries
                .Where(x => x.Item1 == target || x.Item1 == Target.All)
                .Select(x => x.Item2)
                .OrderByDescending(x => x.DateTime)
                .ToArray();
        }
    }
}
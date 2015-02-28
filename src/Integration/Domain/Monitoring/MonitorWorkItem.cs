using System;
using System.Collections.Generic;
using System.Linq;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Domain.Monitoring
{
    public class MonitorWorkItem
    {
        private readonly Range<DateTimeOffset> _checkRange;
	    private readonly List<Tuple<Target, MonitorEntry>> _entries;
	    private readonly List<ISpecification<MonitorEntry>> _ignoreFilters;
        private readonly ChainOfResponsibilityLink<MonitorEntry, Target> _targetRedirects; 

	    public MonitorWorkItem(DateTimeOffset lastRun, bool updateLastCheck)
        {
	        var upperBound = Time.UtcNow;

		    if (lastRun > upperBound)
                upperBound = lastRun;

	        UpdateLastCheck = updateLastCheck;

		    _checkRange = new Range<DateTimeOffset>(lastRun, upperBound);
		    _entries = new List<Tuple<Target, MonitorEntry>>();
	        _ignoreFilters = new List<ISpecification<MonitorEntry>>();
	        _targetRedirects = ChainOfResponsibility.Empty<MonitorEntry, Target>();
        }

        public Range<DateTimeOffset> CheckRange
		{
			get { return _checkRange; }
		}

        public bool UpdateLastCheck { get; private set; }

        public MonitorWorkItem WithIgnoreFilter(ISpecification<MonitorEntry> filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            _ignoreFilters.Add(filter);

            return this;
        }

        public MonitorWorkItem WithTargetRedirect(IChainOfResponsibilityLink<MonitorEntry, Target> redirect)
        {
            if (redirect == null) throw new ArgumentNullException("redirect");

            _targetRedirects.Chain(redirect);

            return this;
        }

        public MonitorWorkItem Add(MonitorEntry entry, Target target)
        {
            if (entry == null) throw new ArgumentNullException("entry");

            if (!_ignoreFilters.Any(filter => filter.IsSatisfiedBy(entry)))
            {
                Target newTarget;
                if (_targetRedirects.TryHandle(entry, out newTarget))
                    target = newTarget;

                _entries.Add(Tuple.Create(target, entry));
            }

            return this;
        }

        public void Add(DateTimeOffset dateTime, string source, string message, Target target)
        {
            Add(new MonitorEntry(dateTime, source, message), target);
        }

        public MonitorEntry[] GetEntries(Target target)
        {
            return _entries
                .Where(x => new[] { target, Target.All }.Any(t => x.Item1.Equals(t)))
                .Select(x => x.Item2)
                .OrderByDescending(x => x.DateTime)
                .ToArray();
        }
    }
}
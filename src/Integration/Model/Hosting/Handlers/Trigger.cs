using System;
using System.Collections.Generic;
using System.Linq;
using TaskScheduler;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public abstract class Trigger
	{
		private readonly DateTimeOffset _startDate;
		private readonly _TASK_TRIGGER_TYPE2 _type;
		private readonly TimeSpan _interval;

		protected Trigger(DateTimeOffset startDate, _TASK_TRIGGER_TYPE2 type, TimeSpan? interval)
		{
			if (interval.HasValue && interval.Value.TotalDays > 31)
				throw new InvalidOperationException("Maximum interval is 31 days");

			if (interval.HasValue && interval.Value.TotalSeconds < 60)
				throw new InvalidOperationException("Minimum interval is 1 minute");

			_startDate = startDate;
			_type = type;
			_interval = interval.GetValueOrDefault();
		}

		public abstract void AddToTask(ITaskDefinition taskDefinition);

		protected T Create<T>(ITaskDefinition taskDefinition) //where T : ITrigger
		{
			ITrigger trigger = taskDefinition.Triggers.Create(_type);
			trigger.Enabled = true;
			trigger.StartBoundary = _startDate.ToString("O");

			if (_interval.TotalSeconds > 0)
			{
				trigger.Repetition.Interval = String.Format("P{0}DT{1}H{2}M{3}S",
					_interval.Days, _interval.Hours, _interval.Minutes, _interval.Seconds);
			}

			return (T)trigger;
		}
	}

	public class OneTimeTrigger : Trigger
	{
		public OneTimeTrigger(DateTimeOffset startDate, TimeSpan? interval = null)
			: base(startDate, _TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME, interval)
		{

		}

		public override void AddToTask(ITaskDefinition taskDefinition)
		{
			ITimeTrigger trigger = Create<ITimeTrigger>(taskDefinition);
		}
	}

	public class DailyTrigger : Trigger
	{
		private readonly short _recureDays;

		public DailyTrigger(DateTimeOffset startDate, short recureDays, TimeSpan? interval = null)
			: base(startDate, _TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY, interval)
		{
			_recureDays = recureDays;
		}

		public override void AddToTask(ITaskDefinition taskDefinition)
		{
			IDailyTrigger trigger = Create<IDailyTrigger>(taskDefinition);
			trigger.DaysInterval = _recureDays;
		}
	}

	public class WeeklyTrigger : Trigger
	{
		private readonly short _weeksInterval;
		private readonly DayOfWeek[] _recurDays;
		private readonly Dictionary<DayOfWeek, short> _bitsForWeekDays;

		public WeeklyTrigger(DateTimeOffset startDate, short weeksInterval, params DayOfWeek[] recurDays)
			: this(startDate, weeksInterval, null, recurDays)
		{

		}

		public WeeklyTrigger(DateTimeOffset startDate, short weeksInterval, TimeSpan? interval = null, params DayOfWeek[] recurDays)
			: base(startDate, _TASK_TRIGGER_TYPE2.TASK_TRIGGER_WEEKLY, interval)
		{
			_weeksInterval = weeksInterval;
			_recurDays = recurDays;

			short bit = 1;
			_bitsForWeekDays =
				Enum.GetValues(typeof(DayOfWeek))
					.Cast<DayOfWeek>()
					.Select(x =>
					{
						short bitValue = bit;
						bit = (short)(bit * 2);
						return new KeyValuePair<DayOfWeek, short>(x, bitValue);
					})
					.ToDictionary(x => x.Key, x => x.Value);
		}

		public override void AddToTask(ITaskDefinition taskDefinition)
		{
			IWeeklyTrigger trigger = Create<IWeeklyTrigger>(taskDefinition);
			trigger.WeeksInterval = _weeksInterval;
			trigger.DaysOfWeek = _recurDays.Distinct().Select(x => _bitsForWeekDays[x]).Aggregate((x, y) => (short)(x | y));
		}
	}
}
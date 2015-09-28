using System;
using System.Collections.Generic;
using System.Linq;
using TaskScheduler;

namespace Vertica.Integration.Infrastructure.Windows
{
	public abstract class ScheduledTaskTrigger
	{
		private readonly DateTimeOffset _startDate;
		private readonly _TASK_TRIGGER_TYPE2 _type;
		private readonly TimeSpan? _interval;

		protected ScheduledTaskTrigger(DateTimeOffset startDate, _TASK_TRIGGER_TYPE2 type, TimeSpan? interval = null)
		{
			if (interval.HasValue && interval.Value.TotalDays > 31) throw new ArgumentOutOfRangeException("interval", @"Maximum interval is 31 days");
			if (interval.HasValue && interval.Value.TotalSeconds < 60) throw new ArgumentOutOfRangeException("interval", @"Minimum interval is 1 minute");

			_startDate = startDate;
			_type = type;
			_interval = interval;
		}

		internal abstract void AddToTask(ITaskDefinition taskDefinition);

		protected T Create<T>(ITaskDefinition task)
		{
			ITrigger trigger = task.Triggers.Create(_type);
			trigger.Enabled = true;
			trigger.StartBoundary = _startDate.ToString("O");

			if (_interval.HasValue)
			{
				trigger.Repetition.Interval = String.Format("P{0}DT{1}H{2}M{3}S",
					_interval.Value.Days, 
					_interval.Value.Hours, 
					_interval.Value.Minutes, 
					_interval.Value.Seconds);
			}

			return (T)trigger;
		}

		public static ScheduledTaskTrigger OneTime(DateTimeOffset when)
		{
			return new OneTimeTrigger(when);
		}

		public static ScheduledTaskTrigger Daily(DateTimeOffset startDate, short recureDays)
		{
			return new DailyTrigger(startDate, recureDays);
		}

		public static ScheduledTaskTrigger Weekly(DateTimeOffset startDate, short weeksInterval)
		{
			return new WeeklyTrigger(startDate, weeksInterval);
		}

		private class OneTimeTrigger : ScheduledTaskTrigger
		{
			public OneTimeTrigger(DateTimeOffset when)
				: base(when, _TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME)
			{
			}

			internal override void AddToTask(ITaskDefinition taskDefinition)
			{
				Create<ITimeTrigger>(taskDefinition);
			}
		}

		private class DailyTrigger : ScheduledTaskTrigger
		{
			private readonly short _recureDays;

			public DailyTrigger(DateTimeOffset startDate, short recureDays, TimeSpan? interval = null)
				: base(startDate, _TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY, interval)
			{
				_recureDays = recureDays;
			}

			internal override void AddToTask(ITaskDefinition taskDefinition)
			{
				IDailyTrigger trigger = Create<IDailyTrigger>(taskDefinition);
				trigger.DaysInterval = _recureDays;
			}
		}

		private class WeeklyTrigger : ScheduledTaskTrigger
		{
			private readonly short _weeksInterval;
			private readonly DayOfWeek[] _recurDays;
			private readonly Dictionary<DayOfWeek, short> _bitsForWeekDays;

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

			internal override void AddToTask(ITaskDefinition taskDefinition)
			{
				IWeeklyTrigger trigger = Create<IWeeklyTrigger>(taskDefinition);
				trigger.WeeksInterval = _weeksInterval;
				trigger.DaysOfWeek = _recurDays.Distinct().Select(x => _bitsForWeekDays[x]).Aggregate((x, y) => (short)(x | y));
			}
		}
	}
}
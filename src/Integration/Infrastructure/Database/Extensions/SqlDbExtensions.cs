using System;
using System.Data.SqlTypes;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Database.Extensions
{
	public static class SqlDbExtensions
	{
		private static readonly Range<DateTime> SqlDateTimeRange = 
			new Range<DateTime>(SqlDateTime.MinValue.Value, SqlDateTime.MaxValue.Value);

		/// <summary>
		/// Ensures that the specific date is within the allowed min/max range of a SQL Server datetime
		/// </summary>
		public static DateTime NormalizeToSqlDateTime(this DateTime dateTime)
		{
			return SqlDateTimeRange.Limit(dateTime);
		}
	}
}
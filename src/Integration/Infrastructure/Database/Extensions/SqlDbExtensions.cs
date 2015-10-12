using System;
using System.Data.SqlTypes;

namespace Vertica.Integration.Infrastructure.Database.Extensions
{
	public static class SqlDbExtensions
	{
		/// <summary>
		/// Ensures that the specific date is within the allowed min/max range of a SQL Server datetime
		/// </summary>
		public static DateTime Normalize(this DateTime dateTime)
		{
			if (dateTime < SqlDateTime.MinValue.Value)
				return SqlDateTime.MinValue.Value;

			if (dateTime > SqlDateTime.MaxValue.Value)
				return SqlDateTime.MaxValue.Value;

			return dateTime;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using Vertica.Integration.Infrastructure.Parsing;

namespace Vertica.Integration.Globase.Csv
{
	public class CsvWriter : ICsvWriter
	{
		public string Write<TEntity>(IEnumerable<CsvRecord<TEntity>> records, Action<CsvRecord<TEntity>> onMap = null)
			where TEntity : CsvRecord<TEntity>
		{
			if (records == null) throw new ArgumentNullException(nameof(records));

			CsvRecord<TEntity>.Field[] fields = CsvRecord<TEntity>.GetFields();

			return CsvRow.BeginRows(fields.Select(x => x.Name).ToArray())
				.Configure(configure => configure
					.ReturnHeaderAsRow()
					.ChangeDelimiter("\t"))
				.FromUsingMapper(records, (mapper, record) =>
				{
					foreach (CsvRecord<TEntity>.Field field in fields)
					{
						onMap?.Invoke(record);

						mapper.Map(field.Name, field.Value(record));
					}
				})
				.ToString();
		}
	}
}
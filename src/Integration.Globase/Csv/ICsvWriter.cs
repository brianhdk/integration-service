using System;
using System.Collections.Generic;

namespace Vertica.Integration.Globase.Csv
{
	public interface ICsvWriter
	{
		string Write<TEntity>(IEnumerable<CsvRecord<TEntity>> records, Action<CsvRecord<TEntity>> onMap)
			where TEntity : CsvRecord<TEntity>;
	}
}
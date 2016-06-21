using System;

namespace Vertica.Integration.Globase.Csv
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class CsvFieldAttribute : Attribute
	{
		public CsvFieldAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}
}
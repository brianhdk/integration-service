using System;
using System.Linq;
using System.Reflection;

namespace Vertica.Integration.Globase.Csv
{
	public abstract class CsvRecord<TEntity>
		where TEntity : CsvRecord<TEntity>
	{
		public TEntity Entity => (TEntity)this;

		internal class Field
		{
			private readonly PropertyInfo _property;
			private readonly CsvFieldAttribute _attribute;

			public Field(PropertyInfo property, CsvFieldAttribute attribute)
			{
				_property = property;
				_attribute = attribute;
			}

			public string Name => _attribute.Name;

			public string Value(CsvRecord<TEntity> instance)
			{
				if (instance == null) throw new ArgumentNullException(nameof(instance));

				return (string)_property.GetValue(instance);
			}
		}

		internal static Field[] GetFields()
		{
			return typeof(TEntity)
				.GetProperties()
				.Where(x => Attribute.IsDefined(x, typeof(CsvFieldAttribute)))
				.Select(x => Tuple.Create(x, x.GetCustomAttribute<CsvFieldAttribute>()))
				.Where(x => x.Item2 != null)
				.Select(x => new Field(x.Item1, x.Item2))
				.ToArray();
		}
	}
}
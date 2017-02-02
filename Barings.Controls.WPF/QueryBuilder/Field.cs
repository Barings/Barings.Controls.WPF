using System;
using System.Collections.Generic;
using System.Reflection;

namespace Barings.Controls.WPF.QueryBuilder
{
	[Serializable]
	public class Field
	{
		public string FieldName { get; set; }

		public Type FieldType { get; set; }

		public IEnumerable<string> ValuesRestrictedTo { get; set; }

		/// <summary>
		/// Default constructor for serialization.
		/// </summary>
		public Field()
		{
		}

		public Field(PropertyInfo propertyInfo)
		{
			FieldType = propertyInfo.PropertyType;
			FieldName = propertyInfo.Name;
			if (propertyInfo.PropertyType == typeof(bool) || propertyInfo.PropertyType == typeof(bool?))
				ValuesRestrictedTo = new[] {"1", "0"};
		}

		public override string ToString()
		{
			return FieldName;
		}

		public override bool Equals(object obj)
		{
			return FieldName == obj?.ToString();
		}
	}
}

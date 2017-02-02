using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Barings.Controls.WPF.QueryBuilder.Attributes;

namespace Barings.Controls.WPF.QueryBuilder.Models
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

			var valueOptionsAttribute =
				propertyInfo.GetCustomAttributes().FirstOrDefault(a => a is ValueOptionsAttribute) as ValueOptionsAttribute;

			if (valueOptionsAttribute != null)
			{
				ValuesRestrictedTo = valueOptionsAttribute.Options;
			}
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

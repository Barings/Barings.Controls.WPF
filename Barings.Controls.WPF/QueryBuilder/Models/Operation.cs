using System;
using System.Collections.Generic;
using System.Linq;
using Barings.Controls.WPF.Extensions;

namespace Barings.Controls.WPF.QueryBuilder.Models
{
	/// <summary>
	/// An operation for use in a query expression.
	/// </summary>
	[Serializable]
	public class Operation
	{
		public string Name { get; set; }

		/// <summary>
		/// A string defining the operation, i.e.: "[field] = [value]".
		/// </summary>
		public string Definition { get; set; }

		/// <summary>
		/// Specifies whether or not this operation requires that a value be present.
		/// </summary>
		public bool RequiresValue { get; set; } = true;

		/// <summary>
		/// A collection of types for which this operation should appear.
		/// </summary>
		public IEnumerable<Type> TypesValidFor;

		public Operation(string name, string definition, params Type[] types)
		{
			Name = name;
			Definition = definition;
			TypesValidFor = types;
		}

		/// <summary>
		/// Returns true if the given type can use this operator.
		/// </summary>
		public bool IsValid(Type type)
		{
			return !TypesValidFor.Any() || TypesValidFor.Contains(type) || TypesValidFor.Contains(Nullable.GetUnderlyingType(type));
		}

		/// <summary>
		/// Gets the appropriate expression based on the inputs
		/// </summary>
		public string GetExpression(Field field, string value)
		{
			string expression;
			var fieldType = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;

			if (fieldType == typeof(string) || fieldType == typeof(DateTime))
				expression = Definition.Replace("[field]", $"[{field}]").Replace("[value]", $"'{value}'");
			else
				expression = Definition.Replace("[field]", $"[{field}]").Replace("[value]", value);

			return expression;
		}

		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object obj)
		{
			return Name == obj?.ToString();
		}

		public static readonly List<Operation> StandardOperations = new List<Operation>
		{
			new Operation("Is Equal To", "[field] = [value]"),
			new Operation("Does Not Equal", "[field] <> [value]", typeof(string), typeof(DateTime)),
			new Operation("Contains", "[field] LIKE '%[value]%'", typeof(string)),
			new Operation("Is Null", "[field] IS NULL") {RequiresValue = false},
			new Operation("Begins With", "[field] LIKE '[value]%'", typeof(string)),
			new Operation("Ends With", "[field] LIKE '%[value]'", typeof(string)),
			new Operation("Is Greater Than Or Equal To", "[field] >= [value]", TypeExtensions.FormattableTypes().ToArray()),
			new Operation("Is Greater Than", "[field] > [value]", TypeExtensions.FormattableTypes().ToArray()),
			new Operation("Is Less Than Or Equal To", "[field] =< [value]", TypeExtensions.FormattableTypes().ToArray()),
			new Operation("Is Less Than", "[field] < [value]", TypeExtensions.FormattableTypes().ToArray())
		};
	}
}

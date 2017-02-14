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
		public string SqlDefinition { get; set; }

		public string LinqDefinition { get; set; }

		/// <summary>
		/// Specifies whether or not this operation requires that a value be present.
		/// </summary>
		public bool RequiresValue { get; set; } = true;

		/// <summary>
		/// A collection of types for which this operation should appear.
		/// </summary>
		public IEnumerable<Type> TypesValidFor;

		public Operation(string name, string sqlDefinition, string linqDefinition, params Type[] types)
		{
			Name = name;
			SqlDefinition = sqlDefinition;
			LinqDefinition = linqDefinition;
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
		/// Gets an expression in SQL syntax.
		/// </summary>
		public string GetSqlExpression(Field field, string value)
		{
			string expression;
			var fieldType = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;

			if (fieldType == typeof(string) || fieldType == typeof(DateTime))
				expression = SqlDefinition.Replace("[field]", $"[{field}]").Replace("[value]", $"'{value}'");
			else
				expression = SqlDefinition.Replace("[field]", $"[{field}]").Replace("[value]", value);

			return expression;
		}

        /// <summary>
        /// Gets an expression to be used in a dynamic Linq query.
        /// </summary>
		public string GetLinqExpression(Field field, string value)
		{
			var fieldType = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;

            // In case our bool comes in as 1/0, convert to true/false
			if (fieldType == typeof(bool)) value = value?.Replace("1", "true").Replace("0", "false");

            // Datetime needs to be in format: DateTime(yyyy, mm, dd)
		    if (fieldType == typeof(DateTime) && !string.IsNullOrWhiteSpace(value))
		        value = $"DateTime({value.Substring(0, 4)}, {value.Substring(4, 2)}, {value.Substring(6, 2)})";

            // String will need quotes around it
		    value = fieldType == typeof(string) && !string.IsNullOrWhiteSpace(value)
		        ? $"\"{value}\""
		        : value ?? "";

		    string fieldSuffix = string.Empty;

            // String comparisons should ignore case
		    if (fieldType == typeof(string))
		    {
		        fieldSuffix = ".ToUpper()";
		    }

            return LinqDefinition.Replace("[field]", field + fieldSuffix).Replace("[value]", value + fieldSuffix);
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
			new Operation("Is Equal To","[field] == [value]" , "[field] = [value]"),
			new Operation("Does Not Equal", "[field] != [value]", "[field] <> [value]", typeof(string), typeof(DateTime)),
			new Operation("Contains", "[field] LIKE '%[value]%'", "[field].Contains([value])", typeof(string)),
			new Operation("Is Null", "[field] IS NULL", "[field] == null") {RequiresValue = false},
			new Operation("Begins With", "[field] LIKE '[value]%'", "[field].StartsWith([value])", typeof(string)),
			new Operation("Ends With", "[field] LIKE '%[value]'", "[field].EndsWith([value])", typeof(string)),
			new Operation("Is Greater Than Or Equal To", "[field] >= [value]", "[field] >= [value]", TypeExtensions.FormattableTypes().ToArray()),
			new Operation("Is Greater Than", "[field] > [value]", "[field] > [value]", TypeExtensions.FormattableTypes().ToArray()),
			new Operation("Is Less Than Or Equal To", "[field] =< [value]", "[field] <= [value]", TypeExtensions.FormattableTypes().ToArray()),
			new Operation("Is Less Than", "[field] < [value]", "[field] < [value]", TypeExtensions.FormattableTypes().ToArray())
		};
	}
}

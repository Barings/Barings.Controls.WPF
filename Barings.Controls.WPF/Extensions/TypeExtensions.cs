using System;
using System.Collections.Generic;

namespace Barings.Controls.WPF.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Indicates whether or not a particular <see cref="Type"/> is formattable or not
        /// </summary>
        public static bool IsFormattable(this Type type)
        {
            return type.IsNumeric() || type == typeof(DateTime) || type == typeof(DateTime?);
        }

        /// <summary>
        /// Indicates whether or not a particular <see cref="Type"/> is numeric or not
        /// </summary>
        public static bool IsNumeric(this Type type)
        {
            return NumericTypes.Contains(type) || NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }

	    public static HashSet<Type> FormattableTypes()
	    {
		    var hash = NumericTypes;
		    hash.Add(typeof(DateTime));
		    return hash;
	    }

        /// <summary>
        /// A collection of numeric <see cref="Type"/>s
        /// </summary>
        public static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(uint),
            typeof(double),
            typeof(decimal),
            typeof(float),
            typeof(long)
        };
    }
}

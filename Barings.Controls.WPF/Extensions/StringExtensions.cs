using System.Globalization;

namespace Barings.Controls.WPF.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Splits a <see cref="string"/> with camelCase/PascalCase into separate spaced words (also replaces _ with space)
        /// </summary>
        /// <param name="input">The <see cref="string"/> to be split</param>
        /// <returns>A split string based on CamelCasing</returns>
        public static string SplitCamelCase(this string input)
        {
            input = System.Text.RegularExpressions.Regex.Replace(input, "(?<=[a-z])([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
            input = System.Text.RegularExpressions.Regex.Replace(input, "(?<=[A-Z])([A-Z])(?=[a-z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
            input = input.Replace('_', ' ');

            return input;
        }

		/// <summary>
		/// Indicates whether the specified string is null, empty, or consists only of white-space characters.
		/// </summary>
		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		public static decimal? TryParseNullableDecimal(this string input)
		{

			decimal temp;
			if (decimal.TryParse(input, out temp))
				return temp;
			return null;
		}

		public static decimal TryParseDecimal(this string input)
		{
			decimal temp;
			decimal.TryParse(input, out temp);
			return temp;
		}

		public static double? TryParseNullableDouble(this string input)
		{

			double temp;
			if (double.TryParse(input.TrimEnd('%'), NumberStyles.Any, null, out temp))
			{
				if (input.EndsWith("%"))
					temp = temp / 100;
				return temp;
			}
			return null;
		}

		public static double TryParseDouble(this string input)
		{
			double temp;
			if (double.TryParse(input.TrimEnd('%'), out temp))
			{
				if (input.EndsWith("%"))
					temp = temp / 100;
				return temp;
			}
			return temp;
		}
	}
}

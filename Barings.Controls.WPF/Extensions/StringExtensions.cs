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
    }
}

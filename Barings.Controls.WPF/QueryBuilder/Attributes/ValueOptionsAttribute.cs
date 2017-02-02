using System;

namespace Barings.Controls.WPF.QueryBuilder.Attributes
{
    /// <summary>
    /// Used to specify a list of value options to present for a given field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ValueOptionsAttribute : Attribute
    {
		public string[] Options { get; private set; }

        /// <param name="options">A list of options to present for this field.</param>
        public ValueOptionsAttribute(params string[] options)
        {
	        Options = options;
        }
    }
}

using System;

namespace Barings.Controls.WPF.QueryBuilder.Attributes
{
    /// <summary>
    /// Used to specify the table that this class represents for building queries.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class QueryTableAttribute : Attribute
    {
		public string TableName { get; private set; }

        /// <param name="tableName">The table that this class represents in the database.</param>
        public QueryTableAttribute(string tableName)
        {
	        TableName = tableName;
        }
    }
}

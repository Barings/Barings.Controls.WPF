using System;

namespace Barings.Controls.WPF.QueryBuilder.Exceptions
{
    /// <summary>
    /// Thrown when a <see cref="QueryExpression"/> is invalid.
    /// </summary>
    public class InvalidQueryExpressionException : Exception
    {
        public InvalidQueryExpressionException(string message) : base(message)
        {
            
        }
    }
}

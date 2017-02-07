using Barings.Controls.WPF.QueryBuilder.Enums;

namespace Barings.Controls.WPF.QueryBuilder.Interfaces
{
	public interface IExpression
	{
	    string ExpressionText(ExpressionType type);
	}
}

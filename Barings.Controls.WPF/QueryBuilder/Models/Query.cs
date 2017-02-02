using System;

namespace Barings.Controls.WPF.QueryBuilder.Models
{
	[Serializable]
	public class Query
	{
		public QueryExpressionGroup RootExpressionGroup { get; set; }
	}
}
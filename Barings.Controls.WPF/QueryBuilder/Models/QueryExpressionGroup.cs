using System;
using System.Collections.Generic;

namespace Barings.Controls.WPF.QueryBuilder.Models
{
	[Serializable]
	public class QueryExpressionGroup
	{
		public string GroupOperator { get; set; }
		public List<QueryExpressionGroup> Groups { get; set; }
		public List<QueryExpression> Expressions { get; set; }
	}
}

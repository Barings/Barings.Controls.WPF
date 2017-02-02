using System;
using System.Collections.Generic;

namespace Barings.Controls.WPF.QueryBuilder.Models
{
	[Serializable]
	public class QueryExpressionGroupData
	{
		public string GroupOperator { get; set; }
		public List<QueryExpressionGroupData> Groups { get; set; }
		public List<QueryExpressionData> Expressions { get; set; }
	}
}

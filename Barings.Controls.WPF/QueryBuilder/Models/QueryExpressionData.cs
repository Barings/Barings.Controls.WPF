using System;
using Barings.Controls.WPF.QueryBuilder.Attributes;

namespace Barings.Controls.WPF.QueryBuilder.Models
{
	[Serializable]
	public class QueryExpressionData
	{
		public Field Field { get; set; }
		public Operation Operation { get; set; }
		public dynamic Value { get; set; }
	}
}

using System;
using Barings.Controls.WPF.QueryBuilder.Attributes;

namespace Barings.Controls.WPF.Examples.Models
{
    [QueryTable("dbo.Assets")]
    public class Asset
    {
        public string IssuerName { get; set; }
        public string AssetName { get; set; }
        public decimal Par { get; set; }
        public decimal MarketValue { get; set; }
        public decimal Price { get; set; }
        public DateTime MaturityDate { get; set; }

        [ValueOptions("Y", "N")]
        public string BabsonDefaultIndicator { get; set; }
        public bool DefaultIndicator { get; set; }
    }
}

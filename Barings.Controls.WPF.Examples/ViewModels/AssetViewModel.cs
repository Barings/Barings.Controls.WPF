using System;
using System.Collections.ObjectModel;
using Barings.Controls.WPF.Examples.Models;
using PropertyChanged;

namespace Barings.Controls.WPF.Examples.ViewModels
{
    [ImplementPropertyChanged]
	public class AssetViewModel
	{
		public ObservableCollection<Asset> Collection { get; set; }
        
		public AssetViewModel()
		{
            Collection = new ObservableCollection<Asset>
            {
		        new Asset { IssuerName = "Test Issuer 1", AssetName = "Test Asset 1", BabsonDefaultIndicator = "N", DefaultIndicator = true, MarketValue = 71.52m, Par = 100, MaturityDate = DateTime.Today.AddDays(-95), Price = 101.52m },
                new Asset { IssuerName = "Test Issuer 1", AssetName = "Test Asset 2", BabsonDefaultIndicator = "Y", DefaultIndicator = true, MarketValue = 131.52m, Par = 100, MaturityDate = DateTime.Today, Price = 101.52m },
                new Asset { IssuerName = "Test Issuer 2", AssetName = "Test Asset 3", BabsonDefaultIndicator = "N", DefaultIndicator = true, MarketValue = 101.52m, Par = 110, MaturityDate = DateTime.Today.AddDays(-29), Price = 101.52m },
                new Asset { IssuerName = "Test Issuer 2", AssetName = "Test Asset 4", BabsonDefaultIndicator = "Y", DefaultIndicator = false, MarketValue = 121.52m, Par = 110, MaturityDate = DateTime.Today, Price = 101.52m },
                new Asset { IssuerName = "Test Issuer 3", AssetName = "Test Asset 5", BabsonDefaultIndicator = "N", DefaultIndicator = false, MarketValue = 89.52m, Par = 100, MaturityDate = DateTime.Today, Price = 101.52m },
                new Asset { IssuerName = "Bob Co.", AssetName = "Test Asset USD", BabsonDefaultIndicator = "N", DefaultIndicator = false, MarketValue = 101.52m, Par = 100, MaturityDate = DateTime.Today, Price = 101.52m },
                new Asset { IssuerName = "Jim Bob Inc.", AssetName = "Test Asset EUR", BabsonDefaultIndicator = "N", DefaultIndicator = false, MarketValue = 101.52m, Par = 110, MaturityDate = DateTime.Today.AddDays(4), Price = 101.52m },
            };
		}
	}
}

using System;
using System.Collections.ObjectModel;
using System.Windows;
using Barings.Controls.WPF.Examples.Models;
using Barings.Controls.WPF.Examples.ViewModels;

namespace Barings.Controls.WPF.Examples
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

        #region Test Data

        const string SavedQuery = @"{
  ""GroupOperator"": ""And"",
  ""Groups"": null,
  ""Expressions"": [
    {
      ""Field"": {
        ""FieldName"": ""IssuerName"",
        ""FieldType"": ""System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"",
        ""ValuesRestrictedTo"": null
      },
      ""Operation"": {
        ""TypesValidFor"": [
          ""System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089""
        ],
        ""Name"": ""Contains"",
        ""SqlDefinition"": ""[field] LIKE '%[value]%'"",
        ""LinqDefinition"": ""[field].Contains([value])"",
        ""RequiresValue"": true
      },
      ""Value"": ""foo""
    }
  ]
}";

        private ObservableCollection<Asset> GetTestAssetCollection()
        {
            return new ObservableCollection<Asset>
            {
                new Asset { IssuerName = "Foo", AssetName = "Test Asset 1", BabsonDefaultIndicator = "N", DefaultIndicator = true, MarketValue = 71.52m, Par = 100, MaturityDate = DateTime.Today.AddDays(-95), Price = 101.52m },
                new Asset { IssuerName = "Bar", AssetName = "Test Asset 2", BabsonDefaultIndicator = "Y", DefaultIndicator = true, MarketValue = 131.52m, Par = 100, MaturityDate = DateTime.Today, Price = 101.52m },
                new Asset { IssuerName = "Snafu", AssetName = "Test Asset 3", BabsonDefaultIndicator = "N", DefaultIndicator = true, MarketValue = 101.52m, Par = 110, MaturityDate = DateTime.Today.AddDays(-29), Price = 101.52m },
                new Asset { IssuerName = "FooBar Industries", AssetName = "Test Asset 4", BabsonDefaultIndicator = "Y", DefaultIndicator = false, MarketValue = 121.52m, Par = 110, MaturityDate = DateTime.Today, Price = 101.52m },
                new Asset { IssuerName = "FUBAR", AssetName = "Test Asset 5", BabsonDefaultIndicator = "N", DefaultIndicator = false, MarketValue = 89.52m, Par = 100, MaturityDate = DateTime.Today, Price = 101.52m },
                new Asset { IssuerName = "Bob Co.", AssetName = "Test Asset USD", BabsonDefaultIndicator = "N", DefaultIndicator = false, MarketValue = 101.52m, Par = 100, MaturityDate = DateTime.Today, Price = 101.52m },
                new Asset { IssuerName = "Jim Bob Inc.", AssetName = "Test Asset EUR", BabsonDefaultIndicator = "N", DefaultIndicator = false, MarketValue = 101.52m, Par = 110, MaturityDate = DateTime.Today.AddDays(4), Price = 101.52m },
            };
        }

        #endregion

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var collection = GetTestAssetCollection();

            var filteredCollection = WPF.QueryBuilder.QueryBuilder.FilterFromSaved(SavedQuery, collection);
            
            MessageBox.Show(filteredCollection.Count.ToString());
            MessageBox.Show(QueryBuilder.DescriptionText());
        }
	}
}

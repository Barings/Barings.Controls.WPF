using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Barings.Controls.WPF.Examples.Annotations;
using Barings.Controls.WPF.Examples.Models;

namespace Barings.Controls.WPF.Examples.ViewModels
{
	public class AssetViewModel : INotifyPropertyChanged
	{
		private ObservableCollection<Asset> _collection = new ObservableCollection<Asset>();
		private List<Asset> _originalCollection;

		public List<Asset> OriginalCollection
		{
			get { return _originalCollection; }
			set
			{
				if (Equals(value, _originalCollection)) return;
				_originalCollection = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Asset> Collection
		{
			get { return _collection; }
			set
			{
				if (Equals(value, _collection)) return;
				_collection = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public AssetViewModel()
		{
			Collection.Add(new Asset { IssuerName = "Test Issuer 1", AssetName = "Test Asset 1", BabsonDefaultIndicator = "N", DefaultIndicator = true, MarketValue = 71.52m, Par = 100, MaturityDate = DateTime.Today.AddDays(-95), Price = 101.52m });
			Collection.Add(new Asset { IssuerName = "Test Issuer 1", AssetName = "Test Asset 2", BabsonDefaultIndicator = "Y", DefaultIndicator = true, MarketValue = 131.52m, Par = 100, MaturityDate = DateTime.Today, Price = 101.52m });
			Collection.Add(new Asset { IssuerName = "Test Issuer 2", AssetName = "Test Asset 3", BabsonDefaultIndicator = "N", DefaultIndicator = true, MarketValue = 101.52m, Par = 110, MaturityDate = DateTime.Today.AddDays(-29), Price = 101.52m });
			Collection.Add(new Asset { IssuerName = "Test Issuer 2", AssetName = "Test Asset 4", BabsonDefaultIndicator = "Y", DefaultIndicator = false, MarketValue = 121.52m, Par = 110, MaturityDate = DateTime.Today, Price = 101.52m });
			Collection.Add(new Asset { IssuerName = "Test Issuer 3", AssetName = "Test Asset 5", BabsonDefaultIndicator = "N", DefaultIndicator = false, MarketValue = 89.52m, Par = 100, MaturityDate = DateTime.Today, Price = 101.52m });
			Collection.Add(new Asset { IssuerName = "Bob Co.", AssetName = "Test Asset USD", BabsonDefaultIndicator = "N", DefaultIndicator = false, MarketValue = 101.52m, Par = 100, MaturityDate = DateTime.Today, Price = 101.52m });
			Collection.Add(new Asset { IssuerName = "Jim Bob Inc.", AssetName = "Test Asset EUR", BabsonDefaultIndicator = "N", DefaultIndicator = false, MarketValue = 101.52m, Par = 110, MaturityDate = DateTime.Today.AddDays(4), Price = 101.52m });


			OriginalCollection = _collection.ToList();
		}
	}
}

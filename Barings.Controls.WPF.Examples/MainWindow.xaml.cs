using System;
using System.Windows;
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

		private void QueryBuilder_OnGoButtonClick(object sender, EventArgs e)
		{
			var viewModel = DataContext as AssetViewModel;
			if (viewModel == null) return;
			
			//Grid.ItemsSource = 
				QueryBuilder.FilterCollection(viewModel.OriginalCollection);
		}
	}
}

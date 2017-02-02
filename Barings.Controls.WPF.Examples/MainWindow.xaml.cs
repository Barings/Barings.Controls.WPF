using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Barings.Controls.WPF.Examples.Models;
using System.Windows;
using Barings.Controls.WPF.Examples.ViewModels;
using Expression = System.Linq.Expressions.Expression;

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

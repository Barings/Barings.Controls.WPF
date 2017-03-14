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

        private void QueryDescriptionButton_OnClick(object sender, RoutedEventArgs e)
        {                        
            MessageBox.Show(QueryBuilder.DescriptionText());
        }

        private void SqlButton_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(QueryBuilder.GetStatement(WPF.QueryBuilder.Enums.ExpressionType.Sql));
        }
    }
}

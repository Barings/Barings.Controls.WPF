﻿using System;
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(QueryBuilder.DescriptionText());
        }
	}
}

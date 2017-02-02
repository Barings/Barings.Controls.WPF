using System.Windows;
using Barings.Controls.WPF.Examples.Models;

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

			QueryBuilder.SetModelType(typeof(Asset));
		}
	}
}

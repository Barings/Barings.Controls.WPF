using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Barings.Controls.WPF.QueryBuilder.Models;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Barings.Controls.WPF.QueryBuilder
{
	/// <summary>
	/// Interaction logic for ZQueryBuilder.xaml
	/// </summary>
	public partial class QueryBuilder : UserControl
	{
		#region PROPERTIES

		private QueryExpressionGroup RootExpressionGroup { get; set; }
		public List<Field> Fields { get; set; }
		public Type ModelType { get; set; }
		public string TableName { get; set; }

		#endregion

		#region CONSTRUCTORS

		public QueryBuilder()
		{
			InitializeComponent();
		}

		#endregion

		#region METHODS

		#region PUBLIC

		/// <summary>
		/// Sets the type by which to populate the fields available for expressions.
		/// </summary>
		public void SetModelType(Type type)
		{
			try
			{
				Fields = new List<Field>();
				ModelType = type;
				TableName = ResolveTableName(type);

				var properties = type.GetProperties();
				foreach (var property in properties)
				{
					Fields.Add(new Field(property));
				}
				InitializeRootExpressionGroup();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Sets the type by which to populate the fields available for expressions.
		/// </summary>
		public void SetModelTypeFromCollection<T>(IEnumerable<T> collection)
		{
			SetModelType(typeof(T));
		}

		public string GetSqlStatement()
		{
			string statement = $"SELECT *\nFROM {TableName}\nWHERE\n";

			statement += RootExpressionGroup.Text();

			return statement;
		}

		public string SaveToString()
		{
			return JsonConvert.SerializeObject(RootExpressionGroup.GetDataObject(), Formatting.Indented);
		}

		public void LoadFromSavedData(string data)
		{
			var expressionGroup = JsonConvert.DeserializeObject<QueryExpressionGroupData>(data);

			RootExpressionGroup = new QueryExpressionGroup(this, true);
			ExpressionStackPanel.Children.Clear();
			ExpressionStackPanel.Children.Add(RootExpressionGroup);

			RootExpressionGroup.LoadFromData(expressionGroup);
		}

		#endregion

		#region PRIVATE

		private void InitializeRootExpressionGroup()
		{
			RootExpressionGroup = new QueryExpressionGroup(this, true);
			RootExpressionGroup.AddExpression();
			ExpressionStackPanel.Children.Add(RootExpressionGroup);
		}

		private string ResolveTableName(Type type)
		{
			var attributes = type.GetCustomAttributes();
			var tableAttribute = attributes.FirstOrDefault(a => a is QueryTableAttribute) as QueryTableAttribute;

			if (tableAttribute == null) return type.Name;
			// else...

			return tableAttribute.TableName;
		}

		#endregion

		#endregion

		#region EVENT HANDLERS

		private void GoButtonOnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var statement = GetSqlStatement();

				MessageBox.Show(statement);
				Clipboard.SetText(statement);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}

		private void ClearExpressionsOnClick(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you sure you want to clear all expressions?", "Are you sure?",
				MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result == MessageBoxResult.Yes)
			{
				ExpressionStackPanel.Children.Clear();
				RootExpressionGroup = null;
				InitializeRootExpressionGroup();
			}
		}

		private void SaveButtonOnClick(object sender, RoutedEventArgs e)
		{
			var data = SaveToString();

			SaveFileDialog dialog = new SaveFileDialog
			{
				Filter = "JSON file (*.json)|*.json",
				FileName = $"{TableName} Query"
			};

			if (dialog.ShowDialog() == true)
			{
				File.WriteAllText(dialog.FileName, data);
			}
			
		}

		private void LoadButtonOnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				OpenFileDialog dialog = new OpenFileDialog {Filter = "JSON file (*.json)|*.json"};

				if (dialog.ShowDialog() == true)
				{
					var text = File.ReadAllText(dialog.FileName);

					LoadFromSavedData(text);
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		#endregion
	}
}

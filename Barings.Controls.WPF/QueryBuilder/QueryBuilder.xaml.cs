using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Barings.Controls.WPF.QueryBuilder.Attributes;
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
		public string TableName { get; set; }
		private Type TheType { get; set; }

		public static readonly DependencyProperty CollectionToFilterProperty = DependencyProperty.Register(
			"CollectionToFilter", typeof(IList), typeof(QueryBuilder), new PropertyMetadata(default(IList), OnCollectionToFilterPropertyChanged));

		public IList CollectionToFilter
		{
			get { return (IList) GetValue(CollectionToFilterProperty); }
			set
			{
				SetValue(CollectionToFilterProperty, value);
			}
		}

		private static void OnCollectionToFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var collection = e.NewValue as IList;

			if (collection == null) return;
			
			var queryBuilder = d as QueryBuilder;

			if (queryBuilder == null || queryBuilder.TheType != null) return;
			
			queryBuilder.TheType = HeuristicallyDetermineType(collection);
			
			if (queryBuilder.TheType != null) queryBuilder.ProcessModelType(queryBuilder.TheType);
		}

		private static Type HeuristicallyDetermineType(IList list)
		{
			var enumerableType =
				list.GetType()
				.GetInterfaces()
				.Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1)
				.FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

			if (enumerableType != null)
				return enumerableType.GenericTypeArguments[0];

			return list.Count == 0 ? null : list[0].GetType();
		}

		#endregion

		#region CONSTRUCTORS

		public QueryBuilder()
		{
			InitializeComponent();
		}

		#endregion



		#region METHODS

		#region PUBLIC

		public void ProcessModelType(Type type)
		{
			Fields = new List<Field>();
			TableName = ResolveTableName(type);

			var properties = type.GetProperties();
			foreach (var property in properties)
			{
				Fields.Add(new Field(property));
			}
			InitializeRootExpressionGroup();
		}

		public string GetSqlStatement()
		{
			string statement = $"SELECT *\nFROM {TableName}\nWHERE\n";

			statement += RootExpressionGroup.Text();

			return statement;
		}

		public string GetLinqStatement()
		{
			var statement = RootExpressionGroup.LinqText();

			return statement;
		}

		public IList FilterCollection<T>(IEnumerable<T> collection)
		{
			var statement = GetLinqStatement();

			IList remainingItems = collection.Where(statement).ToList();
			IList itemsToRemove = new List<object>();
			IList itemsToAdd = new List<object>();

			foreach (var item in CollectionToFilter)
			{
				if (!remainingItems.Contains(item)) itemsToRemove.Add(item);
			}
			foreach (var item in remainingItems)
			{
				if (!CollectionToFilter.Contains(item)) itemsToAdd.Add(item);
			}

			foreach (var item in itemsToRemove)
			{
				CollectionToFilter.Remove(item);
			}
			foreach (var item in itemsToAdd)
			{
				CollectionToFilter.Add(item);
			}

			return CollectionToFilter;
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

		public event EventHandler GoButtonClick;

		private void GoButtonOnClick(object sender, RoutedEventArgs e)
		{
			GoButtonClick?.Invoke(sender, e);
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

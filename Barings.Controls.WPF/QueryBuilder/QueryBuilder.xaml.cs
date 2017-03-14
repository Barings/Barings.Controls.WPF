using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Barings.Controls.WPF.Extensions;
using Barings.Controls.WPF.QueryBuilder.Attributes;
using Barings.Controls.WPF.QueryBuilder.Enums;
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

        private bool IsFiltering { get; set; }
        private bool IsLoading { get; set; }
        private IList OriginalList { get; set; }
        private QueryExpressionGroup RootExpressionGroup { get; set; }
        public List<Field> Fields { get; set; }
        public string TableName { get; set; }
        private Type TheType { get; set; }

        public static readonly DependencyProperty CollectionToFilterProperty = DependencyProperty.Register(
            "CollectionToFilter", typeof(IList), typeof(QueryBuilder),
            new PropertyMetadata(default(IList), OnCollectionToFilterPropertyChanged));

        public IList CollectionToFilter
        {
            get { return (IList)GetValue(CollectionToFilterProperty); }
            set
            {
                SetValue(CollectionToFilterProperty, value);
            }
        }

        private static void OnCollectionToFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var collection = e.NewValue as IList;
            var queryBuilder = d as QueryBuilder;
            if (queryBuilder == null || queryBuilder.TheType != null || collection == null) return;

            //if (queryBuilder.OriginalList == null) queryBuilder.OriginalList = aad;
            queryBuilder.TheType = HeuristicallyDetermineType(collection);
            if (queryBuilder.TheType != null) queryBuilder.ProcessModelType(queryBuilder.TheType);
            if (queryBuilder.OriginalList == null) queryBuilder.OriginalList = collection.JsonCloneObject(queryBuilder.TheType) as IList;
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

        /// <summary>
        /// Returns the text of the query for a given ExpressionType.
        /// </summary>
        public string GetStatement(ExpressionType type)
        {
            return (type == ExpressionType.Sql ? $"SELECT *\nFROM {TableName}\nWHERE\n" : "") + RootExpressionGroup.ExpressionText(type);
        }


        /// <summary>
        /// Filters the current collection.
        /// </summary>
        public void FilterCollection()
        {
            try
            {
                IsFiltering = true;

                var statement = GetStatement(ExpressionType.Linq);

                var ri = OriginalList.Where(statement);
                var remainingItems = ri as IList<object> ?? ri.Cast<object>().ToList();

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
            }
            catch (Exception e)
            {
                if (AutoUpdateCheckBox.IsChecked == true) return;
                MessageBox.Show(e.Message, "Error filtering collection", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsFiltering = false;
            }
        }

        /// <summary>
        /// Returns a serialized string representing the query as it currently is.
        /// </summary>
        public string SaveToString()
        {
            return JsonConvert.SerializeObject(RootExpressionGroup.GetDataObject(), Formatting.Indented);
        }

        /// <summary>
        /// Loads the builder with a saved query (obtained from <see cref="SaveToString()"/>)
        /// </summary>
        public void LoadFromSavedData(string data)
        {
            try
            {
                IsLoading = true;

                var expressionGroup = JsonConvert.DeserializeObject<QueryExpressionGroupData>(data);

                ExpressionStackPanel.Children.Clear();
                InitializeRootExpressionGroup(false);

                RootExpressionGroup.LoadFromData(expressionGroup);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Returns a plain text description of the query filter.
        /// </summary>
        public string DescriptionText()
        {
            return RootExpressionGroup.DescriptionText();
        }
        
        /// <summary>
        /// Accepts a saved query string and a collection and returns the filtered collection.
        /// </summary>
        public static IList FilterFromSaved(string savedQuery, IList collectionToFilter)
        {
            var builder = new QueryBuilder {CollectionToFilter = collectionToFilter};
            builder.LoadFromSavedData(savedQuery);
            builder.FilterCollection();
            return builder.CollectionToFilter;
        }

        #endregion

        #region PRIVATE

        private void InitializeRootExpressionGroup(bool withChild = true)
        {
            RootExpressionGroup = new QueryExpressionGroup(this, true);
            if(withChild) RootExpressionGroup.AddExpression();
            RootExpressionGroup.ExpressionChanged += RootExpressionGroupOnExpressionChanged;
            ExpressionStackPanel.Children.Add(RootExpressionGroup);
        }

        private void RootExpressionGroupOnExpressionChanged(object sender, EventArgs eventArgs)
        {
            if (AutoUpdateCheckBox.IsChecked == false || IsFiltering || IsLoading) return;
            FilterCollection();
        }

        private static string ResolveTableName(MemberInfo type)
        {
            var attributes = type.GetCustomAttributes();
            var tableAttribute = attributes.FirstOrDefault(a => a is QueryTableAttribute) as QueryTableAttribute;

            return tableAttribute == null ? type.Name : tableAttribute.TableName;
            // else...
        }

        #endregion

        #endregion

        #region EVENT HANDLERS

        private void GoButtonOnClick(object sender, RoutedEventArgs e)
        {
            FilterCollection();
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
                CollectionToFilter = OriginalList.JsonCloneObject(TheType) as IList;
            }
        }

        private void SaveButtonOnClick(object sender, RoutedEventArgs e)
        {
            var data = SaveToString();

            var dialog = new SaveFileDialog
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
                var dialog = new OpenFileDialog { Filter = "JSON file (*.json)|*.json" };

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

        private void AutoUpdateCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            RootExpressionGroupOnExpressionChanged(sender, e);
            GoButton.IsEnabled = false;
        }

        private void AutoUpdateCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            GoButton.IsEnabled = true;
        }

        #endregion
    }
}

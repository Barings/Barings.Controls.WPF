﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Barings.Controls.WPF.QueryBuilder.Models;

namespace Barings.Controls.WPF.QueryBuilder
{
	public partial class ZQueryExpression : UserControl, IExpression
	{
		#region EVENTS

		public event EventHandler RemoveClicked;

		public event EventHandler ConvertToGroupClicked;

		#endregion

		#region PROPERTIES

		public ZQueryExpressionGroup ParentGroup { get; set; }

		public string StringValue
		{
			get
			{
				if (ValueTextBox.Visibility == Visibility.Visible)
					return ValueTextBox?.Text;
				if (ValueComboBox.Visibility == Visibility.Visible)
					return ValueComboBox?.SelectedValue?.ToString();
				if (ValueDatePicker.Visibility == Visibility.Visible && ValueDatePicker.SelectedDate.HasValue)
					return ValueDatePicker?.SelectedDate.Value.ToString("yyyyMMdd");

				// else...
				return null;
			}
		}

		public dynamic Value
		{
			get
			{
				if (ValueTextBox.Visibility == Visibility.Visible)
					return ValueTextBox?.Text;
				if (ValueComboBox.Visibility == Visibility.Visible)
					return ValueComboBox?.SelectedValue;
				if (ValueDatePicker.Visibility == Visibility.Visible && ValueDatePicker.SelectedDate.HasValue)
					return ValueDatePicker?.SelectedDate.Value;

				// else...
				return null;
			}
		}

		#endregion

		#region CONSTRUCTORS

		public ZQueryExpression(ZQueryExpressionGroup parentGroup)
		{
			InitializeComponent();

			ParentGroup = parentGroup;
			Style = (Style)FindResource("FadeInStyle");
		}

		#endregion

		#region METHODS

		public void SetFields(IList<Field> fields)
		{
			FieldList.ItemsSource = fields;
		}

		public string Text()
		{
			var fieldIndex = FieldList.SelectedIndex;
			var operationIndex = OperationList.SelectedIndex;

			if (fieldIndex < 0 || operationIndex < 0)
			{
				MakeInvalidAppearance();
				throw new Exception("Expression invalid. Please correct or remove invalid expression.");
			}
			// else...
			MakeValidAppearance();
			

			var field = FieldList?.Items[fieldIndex] as Field;
			var operation = OperationList?.Items[operationIndex] as Operation;

			if (operation != null && operation.RequiresValue && string.IsNullOrWhiteSpace(StringValue))
			{
				MakeInvalidAppearance();
				throw new Exception($"Expression for [{operation.Name}] on field [{field}] requires value.");
			}
			
			var text = operation?.GetExpression(field, StringValue);
			
			return text;
		}

		private void MakeInvalidAppearance()
		{
			ToolTip = "Required values missing.";
			Background = new SolidColorBrush
			{
				Color = Colors.IndianRed,
				Opacity = .5
			};
		}

		private void MakeValidAppearance()
		{
			ToolTip = null;
			Background = null;
		}

		public void RemoveHandlers()
		{
			RemoveClicked = null;
			ConvertToGroupClicked = null;
		}

		public QueryExpression GetDataObject()
		{
			var data = new QueryExpression
			{
				Operation = OperationList?.SelectedItem as Operation,
				Field = FieldList?.SelectedItem as Field,
				Value = Value
			};

			return data;
		}

		#endregion

		#region EVENT HANDLERS

		private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
		{
			EventHandler handler = RemoveClicked;

			handler?.Invoke(this, e);
		}

		private void ConvertToGroupButton_OnClick(object sender, RoutedEventArgs e)
		{
			EventHandler handler = ConvertToGroupClicked;

			handler?.Invoke(this, e);
		}

		private void FieldList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var field = FieldList.SelectedItem as Field;

			if (field == null) return;

			var operations = Operation.StandardOperations;
			
			OperationList.ItemsSource = operations.Where(o => o.IsValid(field.FieldType));
			OperationList.SelectedIndex = 0;

			SetValueInput();
		}

		private void OperationList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SetValueInput();
		}

		public void LoadFromData(QueryExpression data)
		{
			FieldList.SelectedItem = data.Field;
			OperationList.SelectedItem = data.Operation;
			SetValue(data.Value);
		}

		public void SetValue(dynamic value)
		{
			if (value == null) return;
			if (ValueTextBox.Visibility == Visibility.Visible) ValueTextBox.Text = value.ToString();
			if (ValueComboBox.Visibility == Visibility.Visible) ValueComboBox.SelectedItem = value;
			if (ValueDatePicker.Visibility == Visibility.Visible) ValueDatePicker.SelectedDate = value;
		}

		private void SetValueInput()
		{
			var operation = OperationList.SelectedItem as Operation;

			if (operation == null) return;

			var field = FieldList.SelectedItem as Field;

			if (field?.FieldType == typeof(DateTime) || field?.FieldType == typeof(DateTime?))
			{
				ValueTextBox.Visibility = Visibility.Collapsed;
				ValueComboBox.Visibility = Visibility.Collapsed;
				ValueDatePicker.Visibility = Visibility.Visible;
			}
			else
			{
				if (field?.ValuesRestrictedTo != null && field.ValuesRestrictedTo.Any())
				{
					ValueTextBox.Visibility = Visibility.Collapsed;
					ValueComboBox.Visibility = Visibility.Collapsed;

					ValueComboBox.Visibility = Visibility.Visible;
					ValueComboBox.ItemsSource = field.ValuesRestrictedTo;
					ValueComboBox.SelectedItem = field.ValuesRestrictedTo.ToList()[0];
				}
				else
				{
					ValueTextBox.Visibility = Visibility.Visible;

					ValueDatePicker.Visibility = Visibility.Collapsed;
					ValueDatePicker.SelectedDate = null;

					ValueComboBox.Visibility = Visibility.Collapsed;
					ValueComboBox.ItemsSource = null;
				}
			}

			ValueTextBox.IsEnabled = operation.RequiresValue;
			ValueComboBox.IsEnabled = operation.RequiresValue;
			ValueDatePicker.IsEnabled = operation.RequiresValue;

			if (!operation.RequiresValue) ValueComboBox.SelectedItem = null;
		}

		#endregion
	}
}
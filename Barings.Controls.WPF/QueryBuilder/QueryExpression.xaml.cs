using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Barings.Controls.WPF.Extensions;
using Barings.Controls.WPF.QueryBuilder.Enums;
using Barings.Controls.WPF.QueryBuilder.Exceptions;
using Barings.Controls.WPF.QueryBuilder.Interfaces;
using Barings.Controls.WPF.QueryBuilder.Models;

namespace Barings.Controls.WPF.QueryBuilder
{
	public partial class QueryExpression : UserControl, IExpression
	{
		#region EVENTS

		public event EventHandler RemoveClicked;

		public event EventHandler ConvertToGroupClicked;

	    public event EventHandler ExpressionChanged;

		#endregion

		#region PROPERTIES

		public QueryExpressionGroup ParentGroup { get; set; }

		public Field Field => FieldList?.SelectedItem as Field;
		public Operation Operation => OperationList?.SelectedItem as Operation;

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
				if (ValueNumericTextBox.Visibility == Visibility)
					return ValueNumericTextBox?.DecimalValue.ToString(CultureInfo.InvariantCulture);

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
				if (ValueNumericTextBox?.Visibility == Visibility.Visible)
					return ValueNumericTextBox?.DecimalValue;

				// else...
				return null;
			}
		}

		#endregion

		#region CONSTRUCTORS

		public QueryExpression(QueryExpressionGroup parentGroup)
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
        
	    public string ExpressionText(ExpressionType type)
	    {
            ValidateExpression();

	        return type == ExpressionType.Linq
	            ? Operation?.GetLinqExpression(Field, StringValue)
	            : Operation?.GetSqlExpression(Field, StringValue);
	    }

	    public string DescriptionText()
	    {
	        return $"{Field} {Operation?.Name} {Value.ToString()}";
        }
		
		public void ValidateExpression()
		{
		    if (Field == null || Operation == null)
		    {
		        MakeInvalidAppearance();
		        throw new InvalidQueryExpressionException("Expression invalid. Please select both a field and operation.");
		    }
		    // else...
		    MakeValidAppearance();

		    if (Operation != null && Operation.RequiresValue && string.IsNullOrWhiteSpace(StringValue))
		    {
		        MakeInvalidAppearance();
		        throw new InvalidQueryExpressionException($"Expression for [{Operation.Name}] on field [{Field}] requires value.");
		    }
		}

	    public void RemoveHandlers()
		{
			RemoveClicked = null;
			ConvertToGroupClicked = null;
		    ExpressionChanged = null;
		}

		public QueryExpressionData GetDataObject()
		{
			var data = new QueryExpressionData
			{
				Operation = OperationList?.SelectedItem as Operation,
				Field = FieldList?.SelectedItem as Field,
				Value = Value
			};

			return data;
		}

		public void SetValue(dynamic value)
		{
			if (value == null) return;
			if (ValueTextBox.Visibility == Visibility.Visible) ValueTextBox.Text = value.ToString();
			if (ValueComboBox.Visibility == Visibility.Visible) ValueComboBox.SelectedItem = value;
			if (ValueDatePicker.Visibility == Visibility.Visible) ValueDatePicker.SelectedDate = value;
			if (ValueNumericTextBox.Visibility == Visibility.Visible) ValueNumericTextBox.SetValue(value);

            ExpressionChanged?.Invoke(this, EventArgs.Empty);
		}

	    private void SetValueInput()
	    {
	        var operation = OperationList.SelectedItem as Operation;

	        if (operation == null) return;

	        var field = FieldList.SelectedItem as Field;

	        if (field == null) return;

	        // Dates
	        if (field.FieldType == typeof(DateTime) || field.FieldType == typeof(DateTime?))
	        {
	            MakeVisible(ValueDatePicker);
	        }
	        else if (field.FieldType.IsNumeric())
	        {
	            MakeVisible(ValueNumericTextBox);
	            ValueComboBox.ItemsSource = null;
	        }
	        else if (field.ValuesRestrictedTo != null && field.ValuesRestrictedTo.Any())
	        {
	            MakeVisible(ValueComboBox);
	            ValueComboBox.ItemsSource = field.ValuesRestrictedTo;
	            ValueComboBox.SelectedItem = field.ValuesRestrictedTo.ToList()[0];
	        }
	        else
	        {
	            MakeVisible(ValueTextBox);
				
				
	            ValueDatePicker.SelectedDate = null;
	            ValueComboBox.ItemsSource = null;
	        }

	        ValueTextBox.IsEnabled = operation.RequiresValue;
	        ValueComboBox.IsEnabled = operation.RequiresValue;
	        ValueDatePicker.IsEnabled = operation.RequiresValue;
	        ValueNumericTextBox.IsEnabled = operation.RequiresValue;

	        if (!operation.RequiresValue) ValueComboBox.SelectedItem = null;
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

	    private void MakeVisible(Control control)
		{
			List<Control> controls = new List<Control>
			{
				ValueTextBox,
				ValueComboBox,
				ValueDatePicker,
				ValueNumericTextBox
			};

			control.Visibility = Visibility.Visible;

			foreach (var item in controls)
			{
				if(!ReferenceEquals(item, control)) item.Visibility = Visibility.Collapsed;
			}
		}

		#endregion

		#region EVENT HANDLERS

		private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
		{
			RemoveClicked?.Invoke(this, e);
		}

		private void ConvertToGroupButton_OnClick(object sender, RoutedEventArgs e)
		{
			ConvertToGroupClicked?.Invoke(this, e);
		}

		private void FieldList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var field = FieldList.SelectedItem as Field;

			if (field == null) return;

			var operations = Operation.StandardOperations;
			
			OperationList.ItemsSource = operations.Where(o => o.IsValid(field.FieldType));
			OperationList.SelectedIndex = 0;

			SetValueInput();

            ExpressionChanged?.Invoke(this, EventArgs.Empty);
		}

		private void OperationList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SetValueInput();

            ExpressionChanged?.Invoke(this, EventArgs.Empty);
		}

		public void LoadFromData(QueryExpressionData data)
		{
			FieldList.SelectedItem = data.Field;
			OperationList.SelectedItem = data.Operation;
			SetValue(data.Value);
		}

		#endregion

	    private void Value_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	    {
	        ExpressionChanged?.Invoke(this, EventArgs.Empty);
	    }

	    private void ValueTextBox_OnPreviewKeyUp(object sender, KeyEventArgs e)
	    {
	        ExpressionChanged?.Invoke(this, EventArgs.Empty);
	    }
	}
}

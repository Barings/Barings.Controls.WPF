using Barings.Controls.WPF.Extensions;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Barings.Controls.WPF
{
	public class NumericTextBox : TextBox
	{
		private string _numberFormatString;

		/// <summary>
		/// The Format String for displaying the numeric value.  Format is applied after control loses focus.
		/// </summary>
		[Description("The FormatString for displaying the numeric value")]
		[DefaultValue("n2")]
		public string NumberFormatString
		{
			get { return _numberFormatString ?? "n2"; }
			set
			{
				_numberFormatString = value;
				FormatText();
			}
		}

		private bool IsPercent { get; set; }

		public NumericTextBox()
		{
			PreviewTextInput += OnPreviewTextInput;
			LostFocus += OnLostFocus;
			HorizontalContentAlignment = HorizontalAlignment.Right;
		}

		private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			FormatText();
		}

		private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			char character = e.Text.ToCharArray().FirstOrDefault();
			string numberDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

			if (!char.IsDigit(character) && 
				!char.IsControl(character) && 
				e.Text != numberDecimalSeparator &&
				character != '-' &&
				character != ',')
				e.Handled = true;
			
			if (e.Text == numberDecimalSeparator &&
				Text.IndexOf(numberDecimalSeparator, StringComparison.Ordinal) > -1)
				e.Handled = true;
		}

		public void FormatText()
		{
			if (NumberFormatString.IsNullOrWhiteSpace()) return;

			IsPercent = NumberFormatString.Contains("p") || NumberFormatString.Contains("%");

			// Remove the % sign as it screws up our parse
			if (IsPercent) Text = Text.Replace("%", string.Empty);

			decimal value;

			decimal.TryParse(Text, NumberStyles.Any, null, out value);

			// Turn the percentage into a ratio before formatting, as it will multiply by 100
			if (IsPercent) value = value / 100;

			Text = value.ToString(NumberFormatString);
		}

		public void SetValue(decimal? value)
		{
			Text = value.ToString();
			FormatText();
		}

		public void SetValue(double? value)
		{
			Text = value.ToString();
			FormatText();
		}

		public void SetPercentageValue(decimal? value)
		{
			SetValue(value * 100);
		}
		public void SetPercentageValue(double? value)
		{
			SetValue(value * 100);
		}

		public decimal DecimalValue
		{
			get
			{
				decimal value;
				string text = Text.Replace('%', ' ');
				text = text.TrimEnd();

				if (!decimal.TryParse(text, NumberStyles.Any, null, out value)) return 0;

				if (IsPercent)
					return value / 100;

				return value;
			}
		}

		public double DoubleValue
		{
			get
			{
				double value;
				string text = Text.Replace('%', ' ');
				text = text.TrimEnd();

				if (!double.TryParse(text, NumberStyles.Any, null, out value)) return 0;

				if (IsPercent)
					return value / 100;

				return value;
			}
		}
	}
}

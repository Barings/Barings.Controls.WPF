﻿using System;
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Windows;
using System.Windows.Controls;
using Barings.Controls.WPF.QueryBuilder.Enums;
using Barings.Controls.WPF.QueryBuilder.Exceptions;
using Barings.Controls.WPF.QueryBuilder.Interfaces;
using Barings.Controls.WPF.QueryBuilder.Models;
using Enumerable = System.Linq.Enumerable;

namespace Barings.Controls.WPF.QueryBuilder
{
	[Serializable]
	public partial class QueryExpressionGroup : UserControl, IExpression
	{
		#region PROPERTIES

	    private IList<IExpression> NestedExpressions { get; } = new List<IExpression>();
	    private QueryBuilder Builder { get; set; }
	    private bool IsRootGroup { get; set; }

		#endregion

		public QueryExpressionGroup(QueryBuilder parentBuilder, bool isRootGroup = false)
		{
			InitializeComponent();
			Style = (Style) FindResource("FadeInStyle");
			Builder = parentBuilder;
			IsRootGroup = isRootGroup;
			if(isRootGroup) DeleteGroupButton.Visibility = Visibility.Collapsed;
		}

		#region METHODS

		public QueryExpression AddExpression(QueryExpression expression = null, int atIndex = -1)
		{
			if (expression == null) expression = new QueryExpression(this);
			else expression.ParentGroup = this;
			expression.SetFields(Builder.Fields);
			
			expression.RemoveClicked += ExpressionOnRemoveClicked;
			expression.ConvertToGroupClicked += ExpressionOnConvertToGroupClicked;
		    expression.ExpressionChanged += (sender, args) => ExpressionChanged?.Invoke(sender, args);

			if (atIndex >= 0)
			{
				NestedExpressions.Insert(atIndex, expression);
				ExpressionStackPanel.Children.Insert(atIndex, expression);
			}
			else
			{
				NestedExpressions.Add(expression);
				ExpressionStackPanel.Children.Add(expression);
			}

			OnNestedExpressionsChanged();

			return expression;
		}

		private void RemoveExpression(QueryExpression expression)
		{
			expression.RemoveHandlers();
			expression.ParentGroup.NestedExpressions.Remove(expression);
			expression.ParentGroup.ExpressionStackPanel.Children.Remove(expression);

			OnNestedExpressionsChanged();
		}

	    private void AddExpressionGroup(QueryExpressionGroup group = null, int atIndex = -1)
		{
			if(group == null) group = new QueryExpressionGroup(Builder);
			group.AddExpression();
			if(group.NestedExpressions.Count == 1) group.AddExpression();
			group.Deleting += NestedGroupOnDeleting;
			group.ConvertingToExpression += GroupOnConvertingToExpression;
		    group.ExpressionChanged += (sender, args) => ExpressionChanged?.Invoke(sender, args);

			if (atIndex >= 0)
			{
				NestedExpressions.Insert(atIndex, group);
				ExpressionStackPanel.Children.Insert(atIndex, group);
			}
			else
			{
				NestedExpressions.Add(group);
				ExpressionStackPanel.Children.Add(group);
			}
			OnNestedExpressionsChanged();
		}

	    private QueryExpressionGroup AddSingleExpressionGroup(QueryExpressionGroup group = null)
		{
			if(group == null) group = new QueryExpressionGroup(Builder);
			group.Deleting += NestedGroupOnDeleting;
			group.ConvertingToExpression += GroupOnConvertingToExpression;
		    group.ExpressionChanged += (sender, args) => ExpressionChanged?.Invoke(sender, args);

			NestedExpressions.Add(group);
			ExpressionStackPanel.Children.Add(group);

			OnNestedExpressionsChanged();

			return group;
		}

	    private void RemoveExpressionGroup(QueryExpressionGroup group)
		{
			group.RemoveHandlers();

			group.NestedExpressions.Clear();
			group.ExpressionStackPanel.Children.Clear();

			NestedExpressions.Remove(group);
			ExpressionStackPanel.Children.Remove(group);

			OnNestedExpressionsChanged();
		}

		/// <summary>
		/// Converts a group to an expression
		/// </summary>
		private void GroupOnConvertingToExpression(object sender, EventArgs eventArgs)
		{
			try
			{
				var groupToRemove = (QueryExpressionGroup) sender;
				var expressionToAdd = groupToRemove.NestedExpressions[0] as QueryExpression;
				int index = NestedExpressions.IndexOf(groupToRemove);

				RemoveExpressionGroup(groupToRemove);
				AddExpression(expressionToAdd, index);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Converts an expression to a group
		/// </summary>
		private void ExpressionOnConvertToGroupClicked(object sender, EventArgs eventArgs)
		{
			try
			{
				// Get the expression that wants to convert
				var expression = sender as QueryExpression;
				if (expression == null) return;

				int index = NestedExpressions.IndexOf(expression);

				RemoveExpression(expression);
			
				expression.RemoveButton.IsEnabled = true;

				var group = new QueryExpressionGroup(Builder);
				group.AddExpression(expression);
				AddExpressionGroup(group, index);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		public QueryExpressionGroupData GetDataObject()
		{
			var data = new QueryExpressionGroupData {GroupOperator = GroupMenuButton.Content.ToString()};


			foreach (var item in NestedExpressions)
			{
				var group = item as QueryExpressionGroup;

				if (group != null)
				{
					if(data.Groups == null) data.Groups = new List<QueryExpressionGroupData>();
					data.Groups.Add(group.GetDataObject());
					continue;
				}

				var expression = item as QueryExpression;

				if (expression != null)
				{
					if(data.Expressions == null) data.Expressions = new List<QueryExpressionData>();
					data.Expressions.Add(expression.GetDataObject());
				}
			}
			
			return data;
		}

	    public void LoadFromData(QueryExpressionGroupData expressionGroup)
	    {
	        if (!string.IsNullOrEmpty(expressionGroup.GroupOperator))
	            GroupMenuButton.Content = expressionGroup.GroupOperator;

	        foreach (var expression in expressionGroup.Expressions)
	        {
	            var zExpression = AddExpression();
	            zExpression.LoadFromData(expression);
	        }

	        if (expressionGroup.Groups == null) return;

	        foreach (var group in expressionGroup.Groups)
	        {
	            var zExpressionGroup = AddSingleExpressionGroup();
	            zExpressionGroup.LoadFromData(@group);
	        }
	    }

	    public string ExpressionText(ExpressionType type)
	    {
            var text = "(\n";

            int i = 0;

            List<Exception> validationExceptions = new List<Exception>();

            foreach (var expression in NestedExpressions)
            {
                try
                {
                    i++;
                    text += expression.ExpressionText(type) +
                            (i < NestedExpressions.Count ? " " + GroupMenuButton.Content : "") + "\n";
                }
                catch (InvalidQueryExpressionException e)
                {
                    validationExceptions.Add(e);
                }
            }

            if (validationExceptions.Any())
            {
                string message = Enumerable.Aggregate(validationExceptions, string.Empty, (current, exeption) => current + $"{exeption.Message}\n");

                throw new InvalidQueryExpressionException(message);
            }

            return text + ")";
        }

	    public string DescriptionText()
	    {
	        var text = "(";

	        int i = 0;

	        foreach (var expression in NestedExpressions)
	        {
	            i++;
	            text += expression.DescriptionText() +
                    (i < NestedExpressions.Count ? " " + GroupMenuButton.Content : "") + (i < NestedExpressions.Count ? "\n" : "");
	        }

	        return text + ")";
	    }

	    #endregion

		#region EVENTS

	    private void RemoveHandlers()
		{
			Deleting = null;
			ConvertingToExpression = null;
		}

		public event EventHandler Deleting;

		private void Delete(object sender, RoutedEventArgs e)
		{
			EventHandler handler = Deleting;

			handler?.Invoke(this, e);
		}

		public event EventHandler ConvertingToExpression;

        public event EventHandler ExpressionChanged;

		private void ExpressionOnRemoveClicked(object sender, EventArgs eventArgs)
		{
			var expression = sender as QueryExpression;
			if (expression == null) return;
			NestedExpressions.Remove(expression);
			ExpressionStackPanel.Children.Remove(expression);
			OnNestedExpressionsChanged();

			// Convert this group to an expression if there is only one item after removing
			if (!IsRootGroup && NestedExpressions.Count == 1)
			{
				var handler = ConvertingToExpression;
				handler?.Invoke(this, eventArgs);
			}
		}

		#endregion

		#region EVENT HANDLERS

		private void AddItemButtonOnClick(object sender, RoutedEventArgs e)
		{
			AddExpression();
			GroupMenuButton.IsOpen = false;
		}

		private void AddGroupButtonOnClick(object sender, RoutedEventArgs e)
		{
			AddExpressionGroup();
			GroupMenuButton.IsOpen = false;
		}

		/// <summary>
		/// Handles a group within this group being deleted
		/// </summary>
		private void NestedGroupOnDeleting(object sender, EventArgs eventArgs)
		{
			var group = sender as QueryExpressionGroup;
			if (group == null) return;
			NestedExpressions.Remove(group);
			ExpressionStackPanel.Children.Remove(group);
			OnNestedExpressionsChanged();
		}

		private void OnNestedExpressionsChanged()
		{
			if (IsRootGroup && NestedExpressions.Count > 0)
			{
				var firstExpression = NestedExpressions[0] as QueryExpression;
				if (firstExpression != null) firstExpression.RemoveButton.IsEnabled = NestedExpressions.Count > 1;
			}

			GroupMenuButton.Visibility = NestedExpressions.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

			if(NestedExpressions.Count > 1)
				Bracket.Visibility = Visibility.Visible;

			if (NestedExpressions.Count == 1)
			{
				GroupMenuButton.Visibility = Visibility.Collapsed;
				Bracket.Visibility = Visibility.Collapsed;
			}

			if (NestedExpressions.Count == 0)
			{
				Delete(this, new RoutedEventArgs());
			}
            ExpressionChanged?.Invoke(this, EventArgs.Empty);
		}

		private void OperatorButton_OnClick(object sender, RoutedEventArgs e)
		{
			GroupMenuButton.Content = ((MenuItem) sender).Header;
			GroupMenuButton.IsOpen = false;
            ExpressionChanged?.Invoke(this, EventArgs.Empty);
		}

		private void GroupMenuButton_OnClick(object sender, RoutedEventArgs e)
		{
			if(!GroupMenuButton.IsOpen)
				GroupMenuButton.IsOpen = true;
		}
		
		private void DeleteGroupButtonOnClick(object sender, RoutedEventArgs e)
		{
			var handler = Deleting;
			
			GroupMenuButton.IsOpen = false;
			handler?.Invoke(this, e);
		}

		#endregion
	}
}

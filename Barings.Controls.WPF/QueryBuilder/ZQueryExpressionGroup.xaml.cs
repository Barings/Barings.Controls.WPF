using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Barings.Controls.WPF.QueryBuilder.Models;

namespace Barings.Controls.WPF.QueryBuilder
{
	[Serializable]
	public partial class ZQueryExpressionGroup : UserControl, IExpression
	{
		#region PROPERTIES

		public IList<IExpression> NestedExpressions { get; } = new List<IExpression>();
		public ZQueryBuilder Builder { get; set; }
		public bool IsRootGroup { get; private set; }

		#endregion

		public ZQueryExpressionGroup(ZQueryBuilder parentBuilder, bool isRootGroup = false)
		{
			InitializeComponent();
			Style = (Style) FindResource("FadeInStyle");
			Builder = parentBuilder;
			IsRootGroup = isRootGroup;
			if(isRootGroup) DeleteGroupButton.Visibility = Visibility.Collapsed;
		}

		#region METHODS

		public ZQueryExpression AddExpression(ZQueryExpression expression = null, int atIndex = -1)
		{
			if (expression == null) expression = new ZQueryExpression(this);
			else expression.ParentGroup = this;
			expression.SetFields(Builder.Fields);
			
			expression.RemoveClicked += ExpressionOnRemoveClicked;
			expression.ConvertToGroupClicked += ExpressionOnConvertToGroupClicked;

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

		private void RemoveExpression(ZQueryExpression expression)
		{
			expression.RemoveHandlers();
			expression.ParentGroup.NestedExpressions.Remove(expression);
			expression.ParentGroup.ExpressionStackPanel.Children.Remove(expression);

			OnNestedExpressionsChanged();
		}

		public ZQueryExpressionGroup AddExpressionGroup(ZQueryExpressionGroup group = null, int atIndex = -1)
		{
			if(group == null) group = new ZQueryExpressionGroup(Builder);
			group.AddExpression();
			if(group.NestedExpressions.Count == 1) group.AddExpression();
			group.Deleting += NestedGroupOnDeleting;
			group.ConvertingToExpression += GroupOnConvertingToExpression;

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

			return group;
		}

		public ZQueryExpressionGroup AddSingleExpressionGroup(ZQueryExpressionGroup group = null)
		{
			if(group == null) group = new ZQueryExpressionGroup(Builder);
			group.Deleting += NestedGroupOnDeleting;
			group.ConvertingToExpression += GroupOnConvertingToExpression;

			NestedExpressions.Add(group);
			ExpressionStackPanel.Children.Add(group);

			OnNestedExpressionsChanged();

			return group;
		}

		public void RemoveExpressionGroup(ZQueryExpressionGroup group)
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
				var groupToRemove = (ZQueryExpressionGroup) sender;
				var expressionToAdd = groupToRemove.NestedExpressions[0] as ZQueryExpression;
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
				var expression = sender as ZQueryExpression;
				if (expression == null) return;

				int index = NestedExpressions.IndexOf(expression);

				RemoveExpression(expression);
			
				expression.RemoveButton.IsEnabled = true;

				var group = new ZQueryExpressionGroup(Builder);
				group.AddExpression(expression);
				AddExpressionGroup(group, index);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		public QueryExpressionGroup GetDataObject()
		{
			var data = new QueryExpressionGroup {GroupOperator = GroupMenuButton.Content.ToString()};


			foreach (var item in NestedExpressions)
			{
				var group = item as ZQueryExpressionGroup;

				if (group != null)
				{
					if(data.Groups == null) data.Groups = new List<QueryExpressionGroup>();
					data.Groups.Add(group.GetDataObject());
					continue;
				}

				var expression = item as ZQueryExpression;

				if (expression != null)
				{
					if(data.Expressions == null) data.Expressions = new List<QueryExpression>();
					data.Expressions.Add(expression.GetDataObject());
				}
			}
			
			return data;
		}

		public string Text()
		{
			var text = "(\n";
			int i = 0;

			foreach (var expression in NestedExpressions)
			{
				i++;
				text += expression.Text() + (i < NestedExpressions.Count ? " " + GroupMenuButton.Content : "") + "\n";
			}

			return text + ")";
		}

		#endregion

		#region EVENTS

		public void RemoveHandlers()
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

		private void ExpressionOnRemoveClicked(object sender, EventArgs eventArgs)
		{
			var expression = sender as ZQueryExpression;
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
			var group = sender as ZQueryExpressionGroup;
			if (group == null) return;
			NestedExpressions.Remove(group);
			ExpressionStackPanel.Children.Remove(group);
			OnNestedExpressionsChanged();
		}

		private void OnNestedExpressionsChanged()
		{
			if (IsRootGroup && NestedExpressions.Count > 0)
			{
				var firstExpression = NestedExpressions[0] as ZQueryExpression;
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
		}

		private void OperatorButton_OnClick(object sender, RoutedEventArgs e)
		{
			GroupMenuButton.Content = ((MenuItem) sender).Header;
			GroupMenuButton.IsOpen = false;
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

		public ZQueryExpressionGroup LoadFromData(QueryExpressionGroup expressionGroup)
		{
			foreach (var expression in expressionGroup.Expressions)
			{
				var zExpression = AddExpression();
				zExpression.LoadFromData(expression);
			}

			if (expressionGroup.Groups == null) return this;

			foreach (var group in expressionGroup.Groups)
			{
				var zExpressionGroup = AddSingleExpressionGroup();
				zExpressionGroup.GroupMenuButton.Content = group.GroupOperator;
				zExpressionGroup.LoadFromData(group);
			}

			return this;
		}
	}
}

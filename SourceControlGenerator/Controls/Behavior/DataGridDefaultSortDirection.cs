using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SCG.Controls.Behavior
{

    // Source: https://stackoverflow.com/a/37356392
    public static class DataGridDefaultSortDirection
	{
        public static readonly DependencyProperty DirectionProperty = DependencyProperty.RegisterAttached(
    "Direction", typeof(ListSortDirection), typeof(DataGridDefaultSortDirection), new PropertyMetadata(ListSortDirection.Descending, OnDirectionChanged));

        private static void OnDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid grid)
            {
                grid.Sorting += (source, args) => {
                    if (args.Column.SortDirection == null)
                    {
                        // here we check an attached property value of target column
                        var sortDir = (ListSortDirection)args.Column.GetValue(DataGridDefaultSortDirection.DirectionProperty);
                        args.Column.SortDirection = sortDir;
                    }
                };
            }
            else if(d is DataGridColumn col)
            {
                col.SortDirection = GetDirection(d);
            }
        }

        public static void SetDirection(DependencyObject element, ListSortDirection value)
        {
            element.SetValue(DirectionProperty, value);
        }

        public static ListSortDirection GetDirection(DependencyObject element)
        {
            return (ListSortDirection)element.GetValue(DirectionProperty);
        }
    }
}

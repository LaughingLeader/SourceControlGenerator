using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SCG.Controls.Behavior
{
    public interface ICustomSorter : IComparer
    {
        ListSortDirection SortDirection { get; set; }
        string SortMemberPath { get; set; }
    }

    /// <summary>
    /// Source: https://stackoverflow.com/a/18218963
    /// </summary>
    public class DataGridCustomSortBehaviour
    {
        public static readonly DependencyProperty CustomSorterProperty =
            DependencyProperty.RegisterAttached("CustomSorter", typeof(ICustomSorter), typeof(DataGridCustomSortBehaviour));

        public static ICustomSorter GetCustomSorter(DataGridColumn gridColumn)
        {
            return (ICustomSorter)gridColumn.GetValue(CustomSorterProperty);
        }

        public static void SetCustomSorter(DataGridColumn gridColumn, ICustomSorter value)
        {
            gridColumn.SetValue(CustomSorterProperty, value);
        }

        public static readonly DependencyProperty AllowCustomSortProperty =
            DependencyProperty.RegisterAttached("AllowCustomSort", typeof(bool),
            typeof(DataGridCustomSortBehaviour), new UIPropertyMetadata(false, OnAllowCustomSortChanged));

        public static bool GetAllowCustomSort(DataGrid grid)
        {
            return (bool)grid.GetValue(AllowCustomSortProperty);
        }

        public static void SetAllowCustomSort(DataGrid grid, bool value)
        {
            grid.SetValue(AllowCustomSortProperty, value);
        }

        private static void OnAllowCustomSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var existing = d as DataGrid;
            if (existing == null) return;

            var oldAllow = (bool)e.OldValue;
            var newAllow = (bool)e.NewValue;

            if (!oldAllow && newAllow)
            {
                existing.Sorting += HandleCustomSorting;
            }
            else
            {
                existing.Sorting -= HandleCustomSorting;
            }
        }

        private static void HandleCustomSorting(object sender, DataGridSortingEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null || !GetAllowCustomSort(dataGrid)) return;

            // Sanity check
            var sorter = GetCustomSorter(e.Column);
            if (sorter == null) return;

            var direction = (e.Column.SortDirection != ListSortDirection.Ascending)
                                ? ListSortDirection.Ascending
                                : ListSortDirection.Descending;

            e.Column.SortDirection = sorter.SortDirection = direction;
            sorter.SortMemberPath = e.Column.SortMemberPath;

            if (dataGrid.ItemsSource is ListCollectionView listColView)
            {
                listColView.CustomSort = sorter;
                e.Handled = true;
            }
            else
            {
                Log.Here().Activity($"dataGrid.ItemsSource is: {dataGrid.ItemsSource.GetType()}");
            }
            //else if(dataGrid.ItemsSource is IList<IComparable> list)
            //{

            //}

            
        }
    }
}

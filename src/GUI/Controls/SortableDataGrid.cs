using SCG.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace SCG.Controls
{
    public class SortableDataGrid : DataGrid
    {
        // Dictionary to keep SortDescriptions per ItemSource
        private readonly Dictionary<object, List<SortDescription>> m_SortDescriptions =
            new Dictionary<object, List<SortDescription>>();

        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            base.OnSorting(eventArgs);
            UpdateSorting();
        }
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            ICollectionView view = CollectionViewSource.GetDefaultView(newValue);
            if(view != null)
            {
                view.SortDescriptions.Clear();

                // reset SortDescriptions for new ItemSource
                if (m_SortDescriptions.ContainsKey(newValue))
                {
                    foreach (SortDescription sortDescription in m_SortDescriptions[newValue])
                    {
                        view.SortDescriptions.Add(sortDescription);

                        // I need to tell the column its SortDirection,
                        // otherwise it doesn't draw the triangle adornment
                        DataGridColumn column = Columns.FirstOrDefault(c => c.SortMemberPath == sortDescription.PropertyName);
                        if (column != null)
                            column.SortDirection = sortDescription.Direction;
                    }
                }
            }
            
            UpdateIndices();
        }

        // Store SortDescriptions in dictionary
        private void UpdateSorting()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
            m_SortDescriptions[ItemsSource] = new List<SortDescription>(view.SortDescriptions);
        }

        private void UpdateIndices()
        {
            var itemsSource = this.ItemsSource as IEnumerable;
            if (itemsSource != null)
            {
                int index = 0;
                foreach (var item in itemsSource)
                {
                    if (item is IIndexable obj)
                    {
                        obj.Index = index;
                    }
                    index += 1;
                }
            }
        }
    }
}
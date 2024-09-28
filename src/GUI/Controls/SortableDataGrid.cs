using SCG.Interfaces;

using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace SCG.Controls;

public class SortableDataGrid : DataGrid
{
	// Dictionary to keep SortDescriptions per ItemSource
	private readonly Dictionary<object, List<SortDescription>> m_SortDescriptions =
		[];

	protected override void OnSorting(DataGridSortingEventArgs eventArgs)
	{
		base.OnSorting(eventArgs);
		UpdateSorting();
	}
	protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
	{
		base.OnItemsSourceChanged(oldValue, newValue);

		var view = CollectionViewSource.GetDefaultView(newValue);
		if (view != null)
		{
			view.SortDescriptions.Clear();

			// reset SortDescriptions for new ItemSource
			if (m_SortDescriptions.ContainsKey(newValue))
			{
				foreach (var sortDescription in m_SortDescriptions[newValue])
				{
					view.SortDescriptions.Add(sortDescription);

					// I need to tell the column its SortDirection,
					// otherwise it doesn't draw the triangle adornment
					var column = Columns.FirstOrDefault(c => c.SortMemberPath == sortDescription.PropertyName);
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
		var view = CollectionViewSource.GetDefaultView(ItemsSource);
		m_SortDescriptions[ItemsSource] = new List<SortDescription>(view.SortDescriptions);
	}

	private void UpdateIndices()
	{
		var itemsSource = this.ItemsSource as IEnumerable;
		if (itemsSource != null)
		{
			var index = 0;
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
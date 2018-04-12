using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LL.SCG.Controls
{
	class QuickDataGrid : DataGrid
	{
		protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
		{
			//to make sure cell is selected
			var cells = e.AddedCells.FirstOrDefault();
			if (cells != null)
			{
				this.BeginEdit();

			}
			base.OnSelectedCellsChanged(e);
		}
	}
}

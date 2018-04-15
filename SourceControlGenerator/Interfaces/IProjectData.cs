using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.SCG.Interfaces
{
	public interface IProjectData : INotifyPropertyChanged
	{
		string Name { get; set; }

		string ID { get; set; }
	}
}

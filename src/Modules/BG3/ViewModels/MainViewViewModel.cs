using DynamicData;
using DynamicData.Binding;

using SCG.BG3.Models;

using System.Collections.ObjectModel;

namespace SCG.BG3.ViewModels;
public class MainViewViewModel : ReactiveObject
{
	private readonly ReadOnlyObservableCollection<ProjectData> _projects;
	public ReadOnlyObservableCollection<ProjectData> Projects => _projects;

	private readonly SortExpressionComparer<ProjectData> _projectSort = SortExpressionComparer<ProjectData>.Ascending(p => p.Name);

	public MainViewViewModel(SourceCache<ProjectData, string> source)
	{
		source.Connect().ObserveOn(RxApp.MainThreadScheduler).SortAndBind(out _projects, _projectSort).Subscribe();
	}
}

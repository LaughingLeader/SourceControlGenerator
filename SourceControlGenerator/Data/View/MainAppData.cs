using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LL.SCG.Data;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.ComponentModel;
using System.Collections.ObjectModel;
using LL.SCG.Data.View;
using LL.SCG.Core;
using LL.SCG.Commands;
using System.Windows.Input;
using LL.SCG.Data.App;
using LL.SCG.Interfaces;

namespace LL.SCG.Data.View
{
	public class MainAppData : PropertyChangedBase
	{
		public ManagedProjectsData AppProjects { get; set; }

		private AppSettingsData appSettings;

		public AppSettingsData AppSettings
		{
			get { return appSettings; }
			set
			{
				appSettings = value;
				RaisePropertyChanged("AppSettings");
			}
		}
		
		public ObservableCollection<KeywordData> DateKeyList { get; set; }
		public ObservableCollection<KeywordData> ModuleKeyList { get; private set; }

		private IModuleData currentModuleData;

		public IModuleData CurrentModuleData
		{
			get { return currentModuleData; }
			set
			{
				currentModuleData = value;
				RaisePropertyChanged("CurrentModuleData");
			}
		}

		private bool moduleIsLoaded;

		public bool ModuleIsLoaded
		{
			get { return moduleIsLoaded; }
			set
			{
				moduleIsLoaded = value;
				RaisePropertyChanged("ModuleIsLoaded");
			}
		}


		public void SetModuleKeyList(ObservableCollection<KeywordData> keyList)
		{
			ModuleKeyList = keyList;
		}

		public MainAppData()
		{
			ModuleIsLoaded = false;

			DateKeyList = new ObservableCollection<KeywordData>();

			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$Date",
				KeywordValue = "Current Date (Short Format = mm/dd/yyyy)",
				Replace = (o) => { return DateTime.Now.ToString("d"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateYear",
				KeywordValue = DateTime.Now.Year.ToString(),
				Replace = (o) => { return DateTime.Now.Year.ToString(); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateMonth",
				KeywordValue = DateTime.Now.Month.ToString(),
				Replace = (o) => { return DateTime.Now.Month.ToString(); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateDay",
				KeywordValue = DateTime.Now.Day.ToString(),
				Replace = (o) => { return DateTime.Now.Day.ToString(); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateShortLong",
				KeywordValue = DateTime.Now.ToString("D"),
				Replace = (o) => { return DateTime.Now.ToString("D"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateFullShort",
				KeywordValue = DateTime.Now.ToString("f"),
				Replace = (o) => { return DateTime.Now.ToString("f"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateFullLong",
				KeywordValue = DateTime.Now.ToString("F"),
				Replace = (o) => { return DateTime.Now.ToString("F"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateGeneralShort",
				KeywordValue = DateTime.Now.ToString("g"),
				Replace = (o) => { return DateTime.Now.ToString("g"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateGeneralLong",
				KeywordValue = DateTime.Now.ToString("G"),
				Replace = (o) => { return DateTime.Now.ToString("G"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateMonthDay",
				KeywordValue = DateTime.Now.ToString("M"),
				Replace = (o) => { return DateTime.Now.ToString("M"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateMonthYear",
				KeywordValue = DateTime.Now.ToString("Y"),
				Replace = (o) => { return DateTime.Now.ToString("Y"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateTimeShort",
				KeywordValue = DateTime.Now.ToString("t"),
				Replace = (o) => { return DateTime.Now.ToString("t"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateTimeLong",
				KeywordValue = DateTime.Now.ToString("T"),
				Replace = (o) => { return DateTime.Now.ToString("T"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateRoundTrip",
				KeywordValue = DateTime.Now.ToString("O"),
				Replace = (o) => { return DateTime.Now.ToString("O"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateRFC1123",
				KeywordValue = DateTime.Now.ToString("R"),
				Replace = (o) => { return DateTime.Now.ToString("R"); }
			});
			DateKeyList.Add(new KeywordData()
			{
				KeywordName = "$DateUniversal",
				KeywordValue = DateTime.Now.ToString("U"),
				Replace = (o) => { return DateTime.Now.ToString("U"); }
			});
		}
	}
}

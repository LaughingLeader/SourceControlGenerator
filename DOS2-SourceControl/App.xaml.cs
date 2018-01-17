using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using LL.DOS2.SourceControl.Core;

namespace LL.DOS2.SourceControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SettingsController settingsController;

        public App()
        {
            settingsController = new SettingsController();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            settingsController.Start();
        }
    }
}

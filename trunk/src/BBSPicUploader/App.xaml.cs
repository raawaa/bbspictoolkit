using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace BBSPicUploader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var config = ConfigManager.LoadConfig();

            ConfigManager.LoadBoards();
            
            BBSCore.BBS.Init(Helper.GetVersion(), config.BBSconfig);

            var ts = DateTime.Now - config.LastUpdateTime;

            if (ts.TotalDays > 1)
            {
                Helper.LunchAutoUpdate();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ConfigManager.SaveConfig();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.IO;

namespace BBSPicUploader.Update
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Global.IsDailyCheck = false;

            if (e.Args.Length == 0)
            {
                try
                {
                    var ass = Assembly.Load(File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + Global.MainAppName + ".exe"));
                    Global.AppVer = ass.GetName().Version.ToString();                              
                }
                catch
                {
                    Global.AppVer = "0.0.0.0";
                }                
            }
            else
            {
                Global.AppVer = e.Args[0];

                if (e.Args.Length == 2)
                {
                    if (e.Args[1] == "-f")
                    {
                        Global.IsDailyCheck = true;
                    }
                }
            }
        }
    }
}

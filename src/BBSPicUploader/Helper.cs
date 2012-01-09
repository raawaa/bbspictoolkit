using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BBSPicUploader
{
    public class Helper
    {
        public static void LunchAutoUpdate(bool isDailyCheck = true)
        {
            var args = GetVersion();

            try
            {
                var psi = new ProcessStartInfo(AppDomain.CurrentDomain.BaseDirectory + "Update.exe", args);
                Process.Start(psi);
            }
            finally
            {
                ConfigManager.Config.LastUpdateTime = DateTime.Now;
                ConfigManager.SaveConfig();
            }
        }

        public static string GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

    }
}

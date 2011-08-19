using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBSPicUploader.Update
{
    public class Global
    {
        public const string UpdateUrl = "http://update.xxfflower.info/bbspictoolkit/update.xml";

        public const string MainAppName = "BBSPicUploader";

        public static string AppVer { get; set; }

        public static bool IsDailyCheck { get; set; }
    }
}

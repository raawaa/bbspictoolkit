using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBSCore;

namespace BBSPicUploader
{    
    public class UploaderConfig
    {
        public UploaderConfig()
        {
            BBSconfig = new BBSConfig();
        }

        public virtual BBSConfig BBSconfig { get; set; }

        public virtual DateTime LastUpdateTime { get; set; }

        public virtual bool AutoPost { get; set; }

        public virtual string Username { get; set; }

        public virtual string Password { get; set; }
    }
}

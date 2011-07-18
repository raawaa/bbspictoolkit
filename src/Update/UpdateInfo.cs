using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBSPicUploader.Update
{
    public class UpdateInfo
    {
        public virtual string Version { get; set; }

        public virtual string UpdateFileUrl { get; set; }
    }
}

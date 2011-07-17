using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBSCore
{
    public class PicInfo
    {
        public virtual string FullFilename { get; set; }

        public virtual string Url { get; set; }

        public virtual int Star { get; set; }

        public virtual string Description { get; set; }

        public virtual string Text { get; set; }
    }
}

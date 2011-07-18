using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBSCore
{    
    public class BBSConfig
    {
        public virtual bool UseRawSize { get; set; }        

        public virtual int ThumbWidth { get; set; }        
            

        public BBSConfig()
        {
            UseRawSize = false;
            ThumbWidth = 960;
        }
    }
}

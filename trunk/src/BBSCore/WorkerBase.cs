using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBSCore
{
    public abstract class WorkerBase
    {
        public delegate void FinishHanlder(object sender, EventArgs e);

        public event FinishHanlder OnFinished;

        public void Finish()
        {
            if (OnFinished != null)
            {
                OnFinished.Invoke(this, null);
            }
        }
    }
}

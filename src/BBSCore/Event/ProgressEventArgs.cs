﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBSCore.Event
{
    public class ProgressEventArgs : EventArgs
    {
        public double Progress { get; private set; }

        public ProgressEventArgs(double progress)
        {
            Progress = progress;
        }
    }
}

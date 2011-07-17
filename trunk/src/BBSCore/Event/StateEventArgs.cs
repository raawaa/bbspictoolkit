using System;
using BBSCore.Enum;

namespace BBSCore.Event
{
    public class StateEventArgs : EventArgs
    {
        public UploaderState State { get; private set; }

        public StateEventArgs(UploaderState state)
        {
            State = state;
        }
    }
}

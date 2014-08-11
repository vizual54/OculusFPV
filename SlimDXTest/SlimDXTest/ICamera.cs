﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimDXTest
{
    interface ICamera
    {
        void StartCapture();
        event FrameCompleteEventHandler FrameComplete;
    }

    public class FrameCompleteEventArgs : EventArgs
    {
        public IntPtr buffer;
        public int length;
    }
}

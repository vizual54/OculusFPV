using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimDXTest
{
    interface ICameraStream : ICamera
    {
        event FrameCompleteEventHandler FrameComplete2;
    }
}

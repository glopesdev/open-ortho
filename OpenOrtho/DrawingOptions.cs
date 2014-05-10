using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenOrtho
{
    [Flags]
    public enum DrawingOptions
    {
        None           = 0x0,
        Names          = 0x1,
        MainLines      = 0x2,
        ProjectionLines = 0x4,
        DistanceLines  = 0x8
    }
}

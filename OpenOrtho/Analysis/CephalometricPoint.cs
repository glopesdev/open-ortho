using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.ComponentModel;

namespace OpenOrtho.Analysis
{
    public class CephalometricPoint
    {
        public string Name { get; set; }

        [Browsable(false)]
        public bool Placed { get; set; }

        public Vector2 Measurement { get; set; }
    }
}

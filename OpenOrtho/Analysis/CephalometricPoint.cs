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
        public CephalometricPoint()
        {
            Measurement = new Vector2(float.NaN, float.NaN);
        }

        public string Name { get; set; }

        public string Description { get; set; }

        [Browsable(false)]
        public bool Placed { get; set; }

        public Vector2 Measurement { get; set; }
    }
}

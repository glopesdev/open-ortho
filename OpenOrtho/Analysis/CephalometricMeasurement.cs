using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenOrtho.Analysis
{
    public abstract class CephalometricMeasurement
    {
        public string Name { get; set; }

        public abstract string Units { get; }

        public abstract float Measure(CephalometricPointCollection points);
    }
}

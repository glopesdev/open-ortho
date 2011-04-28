using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenOrtho.Analysis
{
    public abstract class CephalometricMeasurement
    {
        protected CephalometricMeasurement()
        {
            Enabled = true;
        }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        public abstract string Units { get; }

        public abstract float Measure(CephalometricPointCollection points, CephalometricMeasurementCollection measurements);
    }
}

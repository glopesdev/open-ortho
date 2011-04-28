using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenOrtho.Analysis
{
    public class DistanceMeasurement : CephalometricMeasurement
    {
        public override string Units
        {
            get { return "mm"; }
        }

        public string Point0 { get; set; }

        public string Point1 { get; set; }

        public override float Measure(CephalometricPointCollection points, CephalometricMeasurementCollection measurements)
        {
            return (points[Point1].Measurement - points[Point0].Measurement).Length;
        }
    }
}

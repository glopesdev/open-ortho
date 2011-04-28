using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenOrtho.Analysis
{
    public class LineDistanceMeasurement : CephalometricMeasurement
    {
        public override string Units
        {
            get { return "mm"; }
        }

        public string Point { get; set; }

        public string Line0 { get; set; }

        public string Line1 { get; set; }

        public override float Measure(CephalometricPointCollection points, CephalometricMeasurementCollection measurements)
        {
            return Utilities.PointLineDistance(points[Point].Measurement, points[Line0].Measurement, points[Line1].Measurement);
        }
    }
}

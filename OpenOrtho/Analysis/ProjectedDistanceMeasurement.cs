using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenOrtho.Analysis
{
    public class ProjectedDistanceMeasurement : CephalometricMeasurement
    {
        public override string Units
        {
            get { return "mm"; }
        }

        public string Point0 { get; set; }

        public string Point1 { get; set; }

        public string Line0 { get; set; }

        public string Line1 { get; set; }

        public override float Measure(CephalometricPointCollection points, CephalometricMeasurementCollection measurements)
        {
            var projection0 = Utilities.PointOnLine(points[Point0].Measurement, points[Line0].Measurement, points[Line1].Measurement);
            var projection1 = Utilities.PointOnLine(points[Point1].Measurement, points[Line0].Measurement, points[Line1].Measurement);
            return (projection1 - projection0).Length;
        }
    }
}

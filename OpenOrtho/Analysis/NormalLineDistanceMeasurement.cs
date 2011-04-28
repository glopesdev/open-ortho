using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenOrtho.Analysis
{
    public class NormalLineDistanceMeasurement : CephalometricMeasurement
    {
        public override string Units
        {
            get { return "mm"; }
        }

        public string Point { get; set; }

        public string NormalLinePoint { get; set; }

        public string Line0 { get; set; }

        public string Line1 { get; set; }

        public override float Measure(CephalometricPointCollection points, CephalometricMeasurementCollection measurements)
        {
            var normal0 = Utilities.PointOnLine(points[NormalLinePoint].Measurement, points[Line0].Measurement, points[Line1].Measurement);
            var normal1 = points[NormalLinePoint].Measurement;
            return Utilities.PointLineDistance(points[Point].Measurement, normal0, normal1);
        }
    }
}

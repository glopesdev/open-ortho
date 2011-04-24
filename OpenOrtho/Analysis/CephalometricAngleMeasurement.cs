﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenOrtho.Analysis
{
    public class CephalometricAngleMeasurement : CephalometricMeasurement
    {
        public string PointA0 { get; set; }

        public string PointA1 { get; set; }

        public string PointB0 { get; set; }

        public string PointB1 { get; set; }

        public override float Measure(CephalometricPointCollection points)
        {
            var lineA = new Vector3(points[PointA1].Measurement - points[PointA0].Measurement);
            var lineB = new Vector3(points[PointB1].Measurement - points[PointB0].Measurement);
            return Vector3.CalculateAngle(lineA, lineB);
        }
    }
}
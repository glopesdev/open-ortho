using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenOrtho.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

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

        public override void Draw(SpriteBatch spriteBatch, CephalometricPointCollection points, CephalometricMeasurementCollection measurements, DrawingOptions options)
        {
            if ((options & DrawingOptions.MainLines) == 0) return;

            if (!string.IsNullOrEmpty(Point0) && !string.IsNullOrEmpty(Point1) &&
                !string.IsNullOrEmpty(Line0) && !string.IsNullOrEmpty(Line1))
            {
                var point0 = points[Point0];
                var point1 = points[Point1];
                var line0 = points[Line0];
                var line1 = points[Line1];

                if (point0.Placed && point1.Placed && line0.Placed && line1.Placed)
                {
                    var projection0 = Utilities.PointOnLine(point0.Measurement, line0.Measurement, line1.Measurement);
                    var projection1 = Utilities.PointOnLine(point1.Measurement, line0.Measurement, line1.Measurement);

                    spriteBatch.DrawVertices(new[] { line0.Measurement, line1.Measurement }, BeginMode.Lines, Color4.Orange);
                    spriteBatch.DrawVertices(new[] { point0.Measurement, projection0, point1.Measurement, projection1 }, BeginMode.Lines, Color4.Blue);
                }
            }
        }
    }
}

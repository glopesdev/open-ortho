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
    public class NormalLineDisplacementMeasurement : CephalometricMeasurement
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
            var line0 = points[Line0].Measurement;
            var line1 = points[Line1].Measurement;
            var normalLinePoint = points[NormalLinePoint].Measurement;

            var normal0 = Utilities.PointOnLine(normalLinePoint, line0, line1);
            var normal1 = normal0 + (line1 - line0).PerpendicularRight;

            return Utilities.PointLineDisplacement(points[Point].Measurement, normal0, normal1);
        }

        public override void Draw(SpriteBatch spriteBatch, CephalometricPointCollection points, CephalometricMeasurementCollection measurements, DrawingOptions options)
        {
            if ((options & DrawingOptions.MainLines) == 0) return;

            if (!string.IsNullOrEmpty(Point) && !string.IsNullOrEmpty(NormalLinePoint) &&
                !string.IsNullOrEmpty(Line0) && !string.IsNullOrEmpty(Line1))
            {
                var point = points[Point];
                var normalLinePoint = points[NormalLinePoint];
                var line0 = points[Line0];
                var line1 = points[Line1];

                if (point.Placed && normalLinePoint.Placed && line0.Placed && line1.Placed)
                {
                    var p = point.Measurement;
                    var l0 = line0.Measurement;
                    var l1 = line1.Measurement;
                    var nlp0 = Utilities.PointOnLine(normalLinePoint.Measurement, line0.Measurement, line1.Measurement);
                    var nlp1 = normalLinePoint.Measurement;
                    var lp = Utilities.PointOnLine(point.Measurement, nlp0, nlp1);

                    spriteBatch.DrawVertices(new[] { l0, nlp0, l1, nlp0, nlp0, nlp1, nlp0, lp, nlp1, lp }, BeginMode.Lines, Color4.Orange);
                    if ((options & DrawingOptions.DistanceLines) != 0)
                    {
                        spriteBatch.DrawVertices(new[] { p, lp }, BeginMode.Lines, Color4.Blue);
                    }
                }
            }
        }
    }
}

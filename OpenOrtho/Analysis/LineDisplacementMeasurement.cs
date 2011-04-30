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
    public class LineDisplacementMeasurement : CephalometricMeasurement
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
            return Utilities.PointLineDisplacement(points[Point].Measurement, points[Line0].Measurement, points[Line1].Measurement);
        }

        public override void Draw(SpriteBatch spriteBatch, CephalometricPointCollection points, CephalometricMeasurementCollection measurements, DrawingOptions options)
        {
            if ((options & DrawingOptions.MainLines) == 0) return;

            if (!string.IsNullOrEmpty(Point) && !string.IsNullOrEmpty(Line0) && !string.IsNullOrEmpty(Line1))
            {
                var point = points[Point];
                var line0 = points[Line0];
                var line1 = points[Line1];
                var projection = Utilities.PointOnLine(point.Measurement, line0.Measurement, line1.Measurement);
                if (point.Placed && line0.Placed && line1.Placed)
                {
                    spriteBatch.DrawVertices(new[] { line0.Measurement, line1.Measurement }, BeginMode.Lines, Color4.Orange);
                    if ((options & DrawingOptions.DistanceLines) != 0)
                    {
                        spriteBatch.DrawVertices(new[] { point.Measurement, projection }, BeginMode.Lines, Color4.Blue);
                    }
                }
            }
        }
    }
}

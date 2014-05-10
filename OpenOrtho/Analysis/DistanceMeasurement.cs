using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenOrtho.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

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

        public override void Draw(SpriteBatch spriteBatch, CephalometricPointCollection points, CephalometricMeasurementCollection measurements, DrawingOptions options)
        {
            if ((options & DrawingOptions.DistanceLines) == 0) return;

            if (!string.IsNullOrEmpty(Point0) && !string.IsNullOrEmpty(Point1))
            {
                var point0 = points[Point0];
                var point1 = points[Point1];
                if (point0.MeasurementSpecified && point1.MeasurementSpecified)
                {
                    spriteBatch.DrawVertices(new[] { point0.Measurement, point1.Measurement }, PrimitiveType.Lines, Color4.Violet);
                }
            }
        }
    }
}

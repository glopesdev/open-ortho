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
    public class AngleMeasurement : CephalometricMeasurement
    {
        const float ExtensionSize = 10;
        List<Vector2> arcPoints = new List<Vector2>(11);

        public override string Units
        {
            get { return "deg"; }
        }

        public string LineA0 { get; set; }

        public string LineA1 { get; set; }

        public string LineB0 { get; set; }

        public string LineB1 { get; set; }

        public override float Measure(CephalometricPointCollection points, CephalometricMeasurementCollection measurements)
        {
            var lineA = points[LineA1].Measurement - points[LineA0].Measurement;
            var lineB = points[LineB1].Measurement - points[LineB0].Measurement;
            return MathHelper.RadiansToDegrees(Utilities.VectorAnglePerpDot(lineB, lineA));
        }

        public override void Draw(SpriteBatch spriteBatch, CephalometricPointCollection points, CephalometricMeasurementCollection measurements, DrawingOptions options)
        {
            if ((options & DrawingOptions.MainLines) == 0) return;

            if (!string.IsNullOrEmpty(LineA0) && !string.IsNullOrEmpty(LineA1) &&
                !string.IsNullOrEmpty(LineB0) && !string.IsNullOrEmpty(LineB1))
            {
                var lineA0 = points[LineA0];
                var lineA1 = points[LineA1];
                var lineB0 = points[LineB0];
                var lineB1 = points[LineB1];

                if (lineA0.MeasurementSpecified && lineA1.MeasurementSpecified && lineB0.MeasurementSpecified && lineB1.MeasurementSpecified)
                {
                    var pA0 = lineA0.Measurement;
                    var pA1 = lineA1.Measurement;
                    var pB0 = lineB0.Measurement;
                    var pB1 = lineB1.Measurement;
                    var intersection = Utilities.LineIntersection(pA0, pA1, pB0, pB1);
                    if (intersection.HasValue)
                    {
                        spriteBatch.DrawVertices(new[]
                        {
                            pA0, pA1,
                            intersection.Value + ExtensionSize * Vector2.Normalize(pA1 - pA0), pA0,
                            pB0, pB1,
                            intersection.Value + ExtensionSize * Vector2.Normalize(pB1 - pB0), pB0
                        }, PrimitiveType.Lines, Color4.Orange);
                    }

                    var angleIncrement = MathHelper.DegreesToRadians(Measure(points, measurements)) / (arcPoints.Capacity - 1);
                    var axis1 = pA0 - pA1;
                    var axis2 = pB0 - pB1;

                    var direction = axis2;
                    direction.Normalize();

                    for (int i = 0; i < arcPoints.Capacity; i++)
                    {
                        arcPoints.Add(intersection.GetValueOrDefault() + direction * 4);
                        direction = Utilities.Rotate(direction, angleIncrement);
                    }

                    spriteBatch.DrawVertices(arcPoints, PrimitiveType.LineStrip, Color4.Orange);
                    arcPoints.Clear();
                }
            }
        }
    }
}

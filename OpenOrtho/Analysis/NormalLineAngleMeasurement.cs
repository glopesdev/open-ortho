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
    public class NormalLineAngleMeasurement : CephalometricMeasurement
    {
        const float ExtensionSize = 10;
        List<Vector2> arcPoints = new List<Vector2>(11);

        public NormalLineAngleMeasurement()
        {
            NormalDirection = NormalDirection.Right;
        }

        public override string Units
        {
            get { return "deg"; }
        }

        public string LineA0 { get; set; }

        public string LineA1 { get; set; }

        public string LineB0 { get; set; }

        public string LineB1 { get; set; }

        public NormalDirection NormalDirection { get; set; }

        public override float Measure(CephalometricPointCollection points, CephalometricMeasurementCollection measurements)
        {
            var lineA0 = points[LineA0].Measurement;
            var lineA1 = points[LineA1].Measurement;
            var lineB0 = points[LineB0].Measurement;
            var lineB1 = points[LineB1].Measurement;
            var normal = NormalDirection == NormalDirection.Left ? (lineA1 - lineA0).PerpendicularLeft : (lineA1 - lineA0).PerpendicularRight;
            var pointNormal = lineA1 + normal;
            var intersection = Utilities.LineIntersection(lineA1, pointNormal, lineB0, lineB1).GetValueOrDefault();
            return MathHelper.RadiansToDegrees(Vector3.CalculateAngle(new Vector3(pointNormal - lineA1), new Vector3(lineB1 - lineB0)));
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
                    var normal = NormalDirection == NormalDirection.Left ? (pA1 - pA0).PerpendicularLeft : (pA1 - pA0).PerpendicularRight;
                    var pointNormal = pA1 + normal;
                    var intersection = Utilities.LineIntersection(pA1, pointNormal, pB0, pB1);
                    if (intersection.HasValue)
                    {
                        spriteBatch.DrawVertices(new[]
                        {
                            pA0, pA1,
                            pA1, pointNormal,
                            intersection.Value + ExtensionSize * Vector2.Normalize(pointNormal - pA1), pA1,
                            pB0, pB1,
                            intersection.Value + ExtensionSize * Vector2.Normalize(pB1 - pB0), pB0
                        }, BeginMode.Lines, Color4.Orange);

                        var angleIncrement = MathHelper.DegreesToRadians(Measure(points, measurements)) / (arcPoints.Capacity - 1);
                        var axis1 = pA1 - pointNormal;
                        var axis2 = pB0 - pB1;

                        var direction = Utilities.CompareClockwise(axis1, axis2) < 0 ? axis1 : axis2;
                        direction.Normalize();

                        for (int i = 0; i < arcPoints.Capacity; i++)
                        {
                            arcPoints.Add(intersection.Value + direction * 4);
                            direction = Utilities.Rotate(direction, angleIncrement);
                        }

                        spriteBatch.DrawVertices(arcPoints, BeginMode.LineStrip, Color4.Orange);
                        arcPoints.Clear();
                    }
                }
            }
        }
    }

    public enum NormalDirection
    {
        Left,
        Right
    }
}

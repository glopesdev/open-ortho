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
    public class NormalLineProjectedDisplacementMeasurement : CephalometricMeasurement
    {
        public override string Units
        {
            get { return "mm"; }
        }

        public string NormalPoint { get; set; }

        public string TargetPoint { get; set; }

        public string LineA0 { get; set; }

        public string LineA1 { get; set; }

        public string LineB0 { get; set; }

        public string LineB1 { get; set; }

        public override float Measure(CephalometricPointCollection points, CephalometricMeasurementCollection measurements)
        {
            var lineA0 = points[LineA0].Measurement;
            var lineA1 = points[LineA1].Measurement;
            var lineB0 = points[LineB0].Measurement;
            var lineB1 = points[LineB1].Measurement;
            var target = points[TargetPoint].Measurement;

            var normal1 = points[NormalPoint].Measurement;
            var normal0 = Utilities.PointOnLine(normal1, lineA0, lineA1);

            var intersection = Utilities.LineIntersection(normal0, normal1, lineB0, lineB1).GetValueOrDefault();
            var targetProjection = Utilities.PointOnLine(target, lineB0, lineB1);
            return Utilities.ScalarProjection(targetProjection - intersection, Vector2.Normalize(lineB1 - lineB0));
        }

        public override void Draw(SpriteBatch spriteBatch, CephalometricPointCollection points, CephalometricMeasurementCollection measurements, DrawingOptions options)
        {
            if ((options & DrawingOptions.MainLines) == 0) return;

            if (!string.IsNullOrEmpty(NormalPoint) && !string.IsNullOrEmpty(TargetPoint) &&
                !string.IsNullOrEmpty(LineA0) && !string.IsNullOrEmpty(LineA1) &&
                !string.IsNullOrEmpty(LineB0) && !string.IsNullOrEmpty(LineB1))
            {
                var target = points[TargetPoint];
                var normal1 = points[NormalPoint];
                var lineA0 = points[LineA0];
                var lineA1 = points[LineA1];
                var lineB0 = points[LineB0];
                var lineB1 = points[LineB1];

                if (target.MeasurementSpecified && normal1.MeasurementSpecified && lineA0.MeasurementSpecified && lineA1.MeasurementSpecified && lineB0.MeasurementSpecified && lineB1.MeasurementSpecified)
                {
                    var pA0 = lineA0.Measurement;
                    var pA1 = lineA1.Measurement;
                    var pB0 = lineB0.Measurement;
                    var pB1 = lineB1.Measurement;
                    var tgt = target.Measurement;
                    var n1 = normal1.Measurement;
                    var n0 = Utilities.PointOnLine(n1, pA0, pA1);
                    var ptgt = Utilities.PointOnLine(tgt, pB0, pB1);

                    var intersection = Utilities.LineIntersection(n0, n1, pB0, pB1);
                    if (intersection.HasValue)
                    {
                        spriteBatch.DrawVertices(new[]
                        {
                            pA0, pA1,
                            n0, intersection.Value,
                            pB0, pB1,
                        }, PrimitiveType.Lines, Color4.Orange);

                        if ((options & DrawingOptions.ProjectionLines) != 0)
                        {
                            spriteBatch.DrawVertices(new[]
                            {
                                n0, intersection.Value,
                                tgt, ptgt
                            }, PrimitiveType.Lines, Color4.Blue);
                        }
                    }
                }
            }
        }
    }
}

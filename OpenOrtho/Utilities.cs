using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenOrtho
{
    public static class Utilities
    {
        public static bool AreEqual(float value1, float value2, float tolerance)
        {
            var diff = value2 - value1;
            return Math.Abs(diff) < tolerance;
        }

        public static float VectorAngle(Vector2 v1, Vector2 v2)
        {
            return Vector3.CalculateAngle(new Vector3(v1), new Vector3(v2));
        }

        public static float LineAngle(Vector3 l1, Vector3 l2)
        {
            return (float)Math.Acos(Math.Abs(l1.X * l2.X + l1.Y * l2.Y + l1.Z * l2.Z) / (l1.Length * l2.Length));
        }

        public static Vector2 Rotate(Vector2 vector, float angle)
        {
            return Vector2.Transform(vector, Quaternion.FromAxisAngle(Vector3.UnitZ, angle));
        }

        public static Vector2 ClosestOnLineExclusive(Vector2 point, Vector2 line0, Vector2 line1)
        {
            var diff0 = point - line0;
            var diff1 = point - line1;
            return diff0.Length < diff1.Length ? line0 : line1;
        }

        public static Vector2 ClosestOnLine(Vector2 point, Vector2 line0, Vector2 line1)
        {
            var diff0 = point - line0;
            var diff1 = point - line1;
            var ldiff0 = diff0.Length - 0.1f;
            var ldiff1 = diff1.Length - 0.1f;
            return (diff0 + diff1).Length < (ldiff0 + ldiff1) ? point : ldiff0 < ldiff1 ? line0 : line1;
        }

        public static Vector2? LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            var x1_x2 = p1.X - p2.X;
            var x3_x4 = p3.X - p4.X;
            var y1_y2 = p1.Y - p2.Y;
            var y3_y4 = p3.Y - p4.Y;
            var x1y2_y1x2 = p1.X * p2.Y - p1.Y * p2.X;
            var x3y4_y3x4 = p3.X * p4.Y - p3.Y * p4.X;
            var divisor = x1_x2 * y3_y4 - y1_y2 * x3_x4;

            return divisor == 0 ? (Vector2?)null : new Vector2(
                (x1y2_y1x2 * x3_x4 - x1_x2 * x3y4_y3x4) / divisor,
                (x1y2_y1x2 * y3_y4 - y1_y2 * x3y4_y3x4) / divisor);
        }

        public static Vector2 VectorIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            var v1 = p2 - p1;
            var v2 = p4 - p3;
            var v3 = p3 - p1;

            var perP1 = v3.X * v2.Y - v3.Y * v2.X;
            var perP2 = v1.X * v2.Y - v1.Y * v2.X;
            var t = perP1 / perP2;

            return new Vector2(p1.X + v1.X * t, p1.Y + v1.Y * t);
        }

        public static Vector2 PointOnLine(Vector2 q, Vector2 p0, Vector2 p1)
        {
            float lineX = p1.X - p0.X;
            float lineY = p1.Y - p0.Y;

            var y = (lineX / ((lineX * lineX) + (lineY * lineY))) *
                    (p0.Y * lineX + lineY * (q.X - p0.X + q.Y * (lineY / lineX)));

            var x = (q.X * lineX + q.Y * lineY - y * lineY) / lineX;

            return new Vector2(x, y);
        }

        public static float PointLineDistance(Vector2 point, Vector2 line0, Vector2 line1)
        {
            return PointLineDistance(new Vector3(point), new Vector3(line0), new Vector3(line1));
        }

        public static float PointLineDistance(Vector3 point, Vector3 line0, Vector3 line1)
        {
            return Vector3.Cross(point - line0, point - line1).Length / (line1 - line0).Length;
        }
    }
}

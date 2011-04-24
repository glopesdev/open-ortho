using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenOrtho
{
    public static class Utilities
    {
        public static Vector2 ClosestOnLine(Vector2 point, Vector2 line0, Vector2 line1)
        {
            // Order line points
            if (line0.X > line1.X)
            {
                var tmp = line0;
                line0 = line1;
                line1 = tmp;
            }

            var diff0 = point - line0;
            var diff1 = point - line1;

            // To the right of p0
            if (diff0.X > 0)
            {
                // To the right of p1
                if (diff1.X > 0) return line1;
                else return point;
            }
            else // To the left of p0
            {
                // To the right of p1
                if (diff1.X > 0) return point;
                else return line0;
            }
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

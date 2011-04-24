using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenOrtho
{
    public static class Utilities
    {
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

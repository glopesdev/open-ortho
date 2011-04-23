using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenOrtho
{
    public static class Utilities
    {
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

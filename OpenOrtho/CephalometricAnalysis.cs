using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenOrtho
{
    public class CephalometricAnalysis
    {
        private readonly List<Vector2> points = new List<Vector2>();

        public List<Vector2> Points
        {
            get { return this.points; }
        }
    }
}

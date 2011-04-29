using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using OpenTK;
using OpenOrtho.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace OpenOrtho.Analysis
{
    public class AngleSumMeasurement : CephalometricMeasurement
    {
        private readonly Collection<string> angles = new Collection<string>();

        public override string Units
        {
            get { return "deg"; }
        }

        public Collection<string> Angles
        {
            get { return this.angles; }
        }

        public override float Measure(CephalometricPointCollection points, CephalometricMeasurementCollection measurements)
        {
            var angleSum = 0f;
            foreach (var angle in angles)
            {
                angleSum += measurements[angle].Measure(points, measurements);
            }

            return angleSum;
        }
    }
}

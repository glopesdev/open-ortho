using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.ComponentModel;
using System.Xml.Serialization;

namespace OpenOrtho.Analysis
{
    public class CephalometricPoint
    {
        Vector2? measurement;

        public string Name { get; set; }

        public string Description { get; set; }

        [Browsable(false)]
        public bool Placed { get; set; }

        public Vector2 Measurement
        {
            get { return measurement.HasValue ? measurement.Value : new Vector2(float.NaN, float.NaN); }
            set { if(!float.IsNaN(value.X) && !float.IsNaN(value.Y)) measurement = value; }
        }

        [Browsable(false)]
        public bool MeasurementSpecified
        {
            get { return Placed && measurement.HasValue; }
        }
    }
}

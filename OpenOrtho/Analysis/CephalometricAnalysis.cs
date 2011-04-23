using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Xml.Serialization;
using System.ComponentModel;

namespace OpenOrtho.Analysis
{
    public class CephalometricAnalysis
    {
        private readonly CephalometricPointCollection points = new CephalometricPointCollection();
        private readonly CephalometricMeasurementCollection measurements = new CephalometricMeasurementCollection();

        [Browsable(false)]
        public string Name { get; set; }

        public CephalometricPointCollection Points
        {
            get { return this.points; }
        }

        [XmlArrayItem("Angle", Type = typeof(CephalometricAngleMeasurement))]
        [XmlArrayItem("Distance", Type = typeof(CephalometricDistanceMeasurement))]
        [XmlArrayItem("LineDistance", Type = typeof(CephalometricLineDistanceMeasurement))]
        public CephalometricMeasurementCollection Measurements
        {
            get { return this.measurements; }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}

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

        [Category("General")]
        public string Name { get; set; }

        [Category("Analysis")]
        public CephalometricPointCollection Points
        {
            get { return this.points; }
        }

        [Category("Analysis")]
        public CephalometricMeasurementCollection Measurements
        {
            get { return this.measurements; }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? "Custom" : Name;
        }
    }
}

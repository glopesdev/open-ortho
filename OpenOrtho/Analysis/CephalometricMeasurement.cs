using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenOrtho.Graphics;
using System.Xml.Serialization;

namespace OpenOrtho.Analysis
{
    public abstract class CephalometricMeasurement
    {
        protected CephalometricMeasurement()
        {
            Enabled = true;
        }

        public string Name { get; set; }

        [XmlIgnore]
        public bool Enabled { get; set; }

        public abstract string Units { get; }

        public abstract float Measure(CephalometricPointCollection points, CephalometricMeasurementCollection measurements);

        public virtual void Draw(SpriteBatch spriteBatch, CephalometricPointCollection points, CephalometricMeasurementCollection measurements, DrawingOptions options)
        {
        }
    }
}

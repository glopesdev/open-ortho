using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OpenOrtho.Analysis;

namespace OpenOrtho
{
    public class OrthoProject
    {
        public OrthoProject()
        {
            PixelsPerMillimeter = 1;
            Analysis = new CephalometricAnalysis();
        }

        public string Radiograph { get; set; }

        public float PixelsPerMillimeter { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public CephalometricAnalysis Analysis { get; set; }
    }
}

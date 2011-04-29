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

        [ReadOnly(true)]
        [Category("Cephalometry")]
        public string Radiograph { get; set; }

        [Category("Scale")]
        public float PixelsPerMillimeter { get; set; }

        [Category("Cephalometry")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public CephalometricAnalysis Analysis { get; set; }
    }
}

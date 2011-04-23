using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenOrtho
{
    public class OrthoProject
    {
        public OrthoProject()
        {
            PixelsPerMeter = 100;
        }

        public string Radiograph { get; set; }

        public float PixelsPerMeter { get; set; }

        public CephalometricAnalysis Analysis { get; set; }
    }
}

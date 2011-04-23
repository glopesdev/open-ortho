using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace OpenOrtho.Analysis
{
    public class CephalometricTemplate
    {
        private readonly Collection<string> points = new Collection<string>();

        public Collection<string> Points
        {
            get { return this.points; }
        }
    }
}

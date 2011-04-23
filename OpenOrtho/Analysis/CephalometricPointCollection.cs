using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace OpenOrtho.Analysis
{
    public class CephalometricPointCollection : KeyedCollection<string, CephalometricPoint>
    {
        protected override string GetKeyForItem(CephalometricPoint item)
        {
            return item.Name;
        }
    }
}

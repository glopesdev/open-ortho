using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;

namespace OpenOrtho.Analysis
{
    [XmlInclude(typeof(AngleMeasurement))]
    [XmlInclude(typeof(AngleSumMeasurement))]
    [XmlInclude(typeof(ConjugateAngleMeasurement))]
    [XmlInclude(typeof(DistanceMeasurement))]
    [XmlInclude(typeof(LineDistanceMeasurement))]
    [XmlInclude(typeof(ProjectedDistanceMeasurement))]
    [XmlInclude(typeof(NormalLineDistanceMeasurement))]
    [Editor(typeof(CephalometricMeasurementCollectionEditor), typeof(UITypeEditor))]
    public class CephalometricMeasurementCollection : KeyedCollection<string, CephalometricMeasurement>
    {
        protected override string GetKeyForItem(CephalometricMeasurement item)
        {
            return item.Name;
        }

        class CephalometricMeasurementCollectionEditor : CollectionEditor
        {
            public CephalometricMeasurementCollectionEditor(Type type)
                : base(type)
            {
            }

            protected override Type[] CreateNewItemTypes()
            {
                return new[]
                {
                    typeof(AngleMeasurement),
                    typeof(AngleSumMeasurement),
                    typeof(ConjugateAngleMeasurement),
                    typeof(DistanceMeasurement),
                    typeof(LineDistanceMeasurement),
                    typeof(ProjectedDistanceMeasurement),
                    typeof(NormalLineDistanceMeasurement)
                };
            }
        }
    }
}

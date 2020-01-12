using System.ComponentModel;
using Phases.DrawableObjects;

namespace Phases.PropertiesCoverters
{
    class ObjectsListConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(DrawableObject.OptionsList((DrawableObject)context.Instance));
        }
    }
}

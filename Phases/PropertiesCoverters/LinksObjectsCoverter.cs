using System.ComponentModel;
using Phases.DrawableObjects;

namespace Phases.PropertiesCoverters
{
    class LinksObjectsCoverter : StringConverter
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
            return new StandardValuesCollection(Link.LinksObjects((SuperTransition)context.Instance));
        }
    }
}

using System.Linq;
using System.ComponentModel;
using Phases.DrawableObjects;

namespace Phases.PropertiesCoverters
{
    class SheetsInBookConverter : StringConverter
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
            return new StandardValuesCollection(DrawingSheet.ChildSheetsNames(((Nested)context.Instance).OwnerDraw.OwnerSheet.OwnerBook.Sheets, ((Nested)context.Instance).OwnerDraw.OwnerSheet));
        }
    }
}

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
            Nested nested = context.Instance as Nested;
            DrawingSheet sheet = nested.OwnerDraw.OwnerSheet;

            return new StandardValuesCollection(sheet.OwnerBook.Models.FindAll(model => model != sheet).ConvertAll(model => model.Name));
        }
    }
}

using System.ComponentModel;
using Phases.DrawableObjects;
using Phases.Variables;

namespace Phases.PropertiesCoverters
{
    class IndirectOutputsList : StringConverter
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
            DrawableObject obj = context.Instance as DrawableObject;
            DrawingSheet sheet = obj.OwnerDraw.OwnerSheet;

            return new StandardValuesCollection(VariableCollection.GetIndirectOutputsList(sheet));
        }
    }
}

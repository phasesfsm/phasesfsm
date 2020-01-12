using System.ComponentModel;
using Phases.DrawableObjects;

namespace Phases.PropertiesCoverters
{
    class IndirectInputsList : StringConverter
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
            return new StandardValuesCollection((context.Instance as Relation).OwnerDraw.OwnerSheet.OwnerBook.Variables.IndirectInputs.ConvertAll(var => var.Name));
        }
    }
}

using System.ComponentModel;
using Phases.DrawableObjects;
using Phases.Variables;

namespace Phases.PropertiesCoverters
{
    class IndirectActionsList : StringConverter
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
            Relation relation = context.Instance as Relation;
            DrawingSheet sheet = relation.OwnerDraw.OwnerSheet;

            return new StandardValuesCollection(VariableCollection.GetIndirectOutputOperations(sheet, relation.Output));
        }
    }
}

using Phases.DrawableObjects;
using Phases.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Phases.PropertiesCoverters
{
    class ConditionalEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            if (value == null) throw new Exception("The property value must be different to null.");
            var svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            var codes = value as string;

            if (svc != null && codes != null)
            {
                DrawableObject obj = context.Instance as DrawableObject;
                DrawingSheet sheet = obj.OwnerDraw.OwnerSheet;

                using (var form = new EditCondition(VariableCollection.GetConditionDictionary(sheet), codes))
                {
                    if (svc.ShowDialog(form) == DialogResult.OK) value = form.Condition;
                }
            }
            return value;
        }
    }
}

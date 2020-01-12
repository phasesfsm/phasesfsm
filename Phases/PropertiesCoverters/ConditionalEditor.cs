using Phases.DrawableObjects;
using System;
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
                using (var form = new EditCondition())
                {
                    var trans = context.Instance as DrawableObject;
                    form.variables = trans.OwnerDraw.OwnerSheet.OwnerBook.Variables.ConditionalVariables;
                    form.tbCondition.Dictionary = form.variables.ToDictionary(kvp => kvp.Name, kvp => kvp.GetImageIndex());
                    form.tbCondition.Text = codes;
                    if (svc.ShowDialog(form) == DialogResult.OK)
                    {
                        value = form.tbCondition.Text;
                    }
                }
            }
            return value;
        }
    }
}

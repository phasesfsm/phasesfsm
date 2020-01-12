using Phases.DrawableObjects;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Phases.PropertiesCoverters
{
    class OutputsEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            var svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            var codes = value as string;

            if (svc != null && codes != null)
            {
                Dictionary<string, int> dictionary;

                dictionary = ((DrawableObject)context.Instance).OwnerDraw.OwnerSheet.OwnerBook.Variables.InternalOutputs.ToDictionary(kvp => kvp.Name, kvp => kvp.GetImageIndex());

                using (var form = new EditOutput(dictionary, value as string))
                {
                    if (svc.ShowDialog(form) == DialogResult.OK)
                    {
                        value = form.Tag.ToString() == "" ? null : form.Tag;
                    }
                }
            }
            return value;
        }
    }
}

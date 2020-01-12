using Phases.DrawableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Phases.Expresions
{
    class CheckMessage
    {
        public enum MessageTypes
        {
            Warning,
            Error
        }

        MessageTypes type;
        string text;
        DrawableObject @object, object2;

        public MessageTypes MessageType => type;
        public string Text => text;
        public DrawableObject Object => @object;
        public DrawableObject Object2 => object2;

        public CheckMessage(MessageTypes messageType, string message, DrawableObject drawableObject, DrawableObject drawableObject2 = null)
        {
            type = messageType;
            text = message;
            @object = drawableObject;
            object2 = drawableObject2;
        }

        public ListViewItem GetListViewItem()
        {
            ListViewItem item = new ListViewItem(text, (int)type);
            if (@object != null)
            {
                item.SubItems.Add(@object.Name);
                item.SubItems.Add(object2 != null ? object2.Name : "");
                item.SubItems.Add(@object.OwnerDraw.OwnerSheet.Name);
            }
            item.Tag = this;
            return item;
        }
    }
}

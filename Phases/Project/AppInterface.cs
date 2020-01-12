using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Phases
{
    class AppInterface
    {
        public PictureBox display;
        public HScrollBar hScroll;
        public VScrollBar vScroll;
        public TreeView view;
        public MouseTool mouse;

        public AppInterface(PictureBox pictureBox, HScrollBar hScrollBar, VScrollBar vScrollBar,
            TreeView treeView, MouseTool mouseTool)
        {
            display = pictureBox;
            hScroll = hScrollBar;
            vScroll = vScrollBar;
            view = treeView;
            mouse = mouseTool;
        }
    }
}

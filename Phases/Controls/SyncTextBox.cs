using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Phases.Controls
{
    class SyncTextBox : RichTextBox
    {
        private System.ComponentModel.IContainer _components = null;

        [
            System.ComponentModel.Category("Syncronization"),
            System.ComponentModel.Description("Gets or sets the child SyncTextBox to syncronize with.")
            ]
        public SyncTextBox SyncChild
        {
            get
            {
                return syncChild;
            }
            set
            {
                syncChild = value;
                if (syncChild != null)
                    syncChild.SyncParent = this;
            }
        }
        private SyncTextBox syncChild;

        [
            System.ComponentModel.Category("Syncronization"),
            System.ComponentModel.Description("Gets parent SyncTextBox that is syncronizing with this.")
            ]
        public SyncTextBox SyncParent{ get; private set; }

        public SyncTextBox()
        {
            ReinitializeCanvas();

            AcceptsTab = true;
            AutoWordSelection = true;
            Font = new Font("Courier New", 10f);
            HideSelection = false;
            ScrollBars = RichTextBoxScrollBars.ForcedBoth;
            WordWrap = false;

            _components = new System.ComponentModel.Container();
            // Calculate width of "W" to set;the small horizontal increment
            OnFontChanged(null);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
                _components.Dispose();
            base.Dispose(disposing);
        }

        private int selStart, selLine, selDir;
        private bool blockSel = false;      

        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);
            if (SelectionLength == 0)   // Just position change
            {
                selDir = 0;
            }
            else if (selStart == SelectionStart) // Positive selection
            {
                selDir = 1;
            }
            else
            {
                selDir = -1;
            }
            selStart = SelectionStart;
            int line = GetLineFromCharIndex(selDir <= 0 ? SelectionStart : SelectionStart + SelectionLength);
            if (syncChild != null && !childLock)
            {
                ProcessSelection(syncChild, line);
            }
            if (SyncParent != null && !childLock)
            {
                ProcessSelection(SyncParent, line);
            }
            selLine = line;
        }

        private void ProcessSelection(SyncTextBox toSync, int line)
        {
            if (!blockSel && line != selLine)
            {
                toSync.childLock = true;
                toSync.SelectionLength = 0;
                int childIndex = toSync.GetFirstCharIndexFromLine(line);
                if (line < toSync.Lines.Length && childIndex >= 0)
                    toSync.SelectionStart = childIndex;
                toSync.childLock = false;
            }
            int thisScroll = Win32.GetScrollPos(Handle, Orientation.Horizontal);
            int childScroll = Win32.GetScrollPos(toSync.Handle, Orientation.Horizontal);
            if (childScroll != thisScroll)
            {
                toSync.childLock = true;
                SetHScroll(toSync, thisScroll);
                toSync.childLock = false;
            }
        }

        public new event EventHandler<PaintEventArgs> Paint;
        //private WindowExtender mWindowExtender;

        //An off-screen image to draw to
        private Bitmap mCanvas;
        //The graphics object associated with the canvas bitmap
        private Graphics mBufferGraphics;
        //The clip region of the canvas graphics (visible control bounds)
        private Rectangle mBufferClip;
        //The graphics object associated with the RichTextBox
        private Graphics mControlGraphics;
        //Flag ensuring that the control can be drawn to
        private bool mCanRender = false;

        //Draws the result of the event handler to the textbox
        protected void OnPerformPaint()
        {
            if (mCanRender)
            {
                //clear the canvas
                mBufferGraphics.Clear(Color.Transparent);
                //give the event handler a chance to draw to the buffer
                OnPaint(new PaintEventArgs(mBufferGraphics, mBufferClip));
                //render the buffer contents to the RichTextBox
                mControlGraphics.DrawImageUnscaled(mCanvas, 0, 0);
            }
        }

        protected void ReinitializeCanvas()
        {
            lock (this)
            {
                TearDown();
                if (Width > 0 && Height > 0)
                {
                    mCanRender = true;
                    mCanvas = new Bitmap(Width, Height);
                    mBufferGraphics = Graphics.FromImage(mCanvas);
                    mBufferClip = ClientRectangle;
                    mBufferGraphics.Clip = new Region(mBufferClip);
                    mControlGraphics = Graphics.FromHwnd(Handle);
                }
                else
                {
                    mCanRender = false;
                }
            }
        }

        protected void TearDown()
        {
            if (mControlGraphics != null)
                mControlGraphics.Dispose();
            if (mBufferGraphics != null)
                mBufferGraphics.Dispose();
            if (mCanvas != null)
                mCanvas.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Paint != null)
                Paint(this, e);
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ReinitializeCanvas();
        }

        private bool childLock = false;

        // Fire scroll event if the scroll-bars are moved
        protected override void WndProc(ref Message message)
        {
            if (message.Msg == Win32.WM_PAINT)
            {
                //Refresh the control//s display
                Invalidate();
                //Perform default drawing
                base.WndProc(ref message);
                //Perform custom drawing
                OnPerformPaint();
            }
            else if (syncChild != null && !childLock)
            {
                if (ProcessMessage(syncChild, ref message))
                    base.WndProc(ref message);
            }
            else if (SyncParent != null && !childLock)
            {
                if (ProcessMessage(SyncParent, ref message))
                    base.WndProc(ref message);
            }
            else
            {
                base.WndProc(ref message);
            }
        }

        public int VerticalScroll => Win32.GetScrollPos((IntPtr)Handle, Orientation.Vertical);

        private bool ProcessMessage(SyncTextBox toSync, ref Message message)
        {
            var wParam = new Win32.WParam(message.WParam);
            switch (message.Msg)
            {
                case Win32.WM_KEYDOWN:
                case Win32.WM_KEYUP:
                    switch (wParam.Message)
                    {
                        case 0x21:
                        case 0x22:
                            blockSel = true;
                            base.WndProc(ref message);
                            message.HWnd = toSync.Handle;
                            toSync.childLock = true;
                            toSync.WndProc(ref message);
                            toSync.childLock = false;
                            blockSel = false;
                            return false;
                    }
                    break;
                case Win32.WM_VSCROLL:
                    switch ((Win32.ScrollBarCommands)wParam.Message)
                    {
                        case Win32.ScrollBarCommands.SB_LINEUP:
                        case Win32.ScrollBarCommands.SB_LINEDOWN:
                        case Win32.ScrollBarCommands.SB_PAGEUP:
                        case Win32.ScrollBarCommands.SB_PAGEDOWN:
                            base.WndProc(ref message);
                            message.HWnd = toSync.Handle;
                            toSync.childLock = true;
                            toSync.WndProc(ref message);
                            toSync.childLock = false;
                            return false;
                        case Win32.ScrollBarCommands.SB_THUMBTRACK:
                            toSync.childLock = true;
                            SetVScroll(toSync, wParam.Value);
                            toSync.childLock = false;
                            break;
                        case Win32.ScrollBarCommands.SB_THUMBPOSITION:
                        case Win32.ScrollBarCommands.SB_ENDSCROLL:
                            break;
                        default:
                            break;
                    }
                    break;
                case Win32.WM_HSCROLL:
                    switch ((Win32.ScrollBarCommands)wParam.Message)
                    {
                        case Win32.ScrollBarCommands.SB_LINELEFT:
                        case Win32.ScrollBarCommands.SB_LINERIGHT:
                        case Win32.ScrollBarCommands.SB_PAGELEFT:
                        case Win32.ScrollBarCommands.SB_PAGERIGHT:
                            base.WndProc(ref message);
                            message.HWnd = toSync.Handle;
                            toSync.childLock = true;
                            toSync.WndProc(ref message);
                            toSync.childLock = false;
                            return false;
                        case Win32.ScrollBarCommands.SB_THUMBTRACK:
                            toSync.childLock = true;
                            SetHScroll(toSync, wParam.Value);
                            toSync.childLock = false;
                            break;
                        case Win32.ScrollBarCommands.SB_THUMBPOSITION:
                        case Win32.ScrollBarCommands.SB_ENDSCROLL:
                            break;
                        default:
                            break;
                    }
                    break;
                case Win32.WM_MOUSEWHEEL:
                    int thisScroll = Win32.GetScrollPos((IntPtr)Handle, Orientation.Vertical);
                    thisScroll -= (wParam.Value / 40) * (PreferredSize.Height / Lines.Length - 1);
                    if (thisScroll < 0) thisScroll = 0;
                    SetVScroll(this, thisScroll);
                    toSync.childLock = true;
                    SetVScroll(toSync, thisScroll);
                    toSync.childLock = false;
                    return false;
            }
            return true;
        }

        private void SetVScroll(SyncTextBox toSync, int value)
        {
            var wParam = new Win32.WParam((int)Win32.ScrollBarCommands.SB_THUMBPOSITION, value);
            var msg = new Message();
            msg.HWnd = toSync.Handle;
            msg.Msg = Win32.WM_VSCROLL;
            msg.WParam = (IntPtr)wParam.Ptr();
            msg.LParam = (IntPtr)0;
            toSync.WndProc(ref msg);
        }

        private void SetHScroll(SyncTextBox toSync, int value)
        {
            var wParam = new Win32.WParam((int)Win32.ScrollBarCommands.SB_THUMBPOSITION, value);
            var msg = new Message();
            msg.HWnd = toSync.Handle;
            msg.Msg = Win32.WM_HSCROLL;
            msg.WParam = (IntPtr)wParam.Ptr();
            msg.LParam = (IntPtr)0;
            toSync.WndProc(ref msg);
        }

        private static class Win32
        {
            public const int WM_PAINT = 0x00F;
            public const int WM_HSCROLL = 0x114;
            public const int WM_VSCROLL = 0x115;
            public const int WM_MOUSEWHEEL = 0x20A;
            public const int WM_KEYDOWN = 0x0100;
            public const int WM_KEYUP = 0x0101;
            public enum ScrollBarCommands : int
            {
                SB_LINEUP = 0,
                SB_LINELEFT = 0,
                SB_LINEDOWN = 1,
                SB_LINERIGHT = 1,
                SB_PAGEUP = 2,
                SB_PAGELEFT = 2,
                SB_PAGEDOWN = 3,
                SB_PAGERIGHT = 3,
                SB_THUMBPOSITION = 4,
                SB_THUMBTRACK = 5,
                SB_TOP = 6,
                SB_LEFT = 6,
                SB_BOTTOM = 7,
                SB_RIGHT = 7,
                SB_ENDSCROLL = 8
            }

            [DllImport("user32.dll")]
            public static extern int SetScrollPos(System.IntPtr hWnd, Orientation nBar, int nPos, bool bRedraw);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int GetScrollPos(IntPtr hWnd, Orientation nBar);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetScrollInfo(IntPtr hwnd, Orientation nBar, ref SCROLLINFO lpsi);

            [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetScrollBarInfo")]
            public static extern int GetScrollBarInfo(IntPtr hWnd, ObjId idObject, ref SCROLLBARINFO psbi);

            [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
            public static extern bool PostMessage(System.IntPtr hWnd, uint msg, int wParam, int lParam);

            public enum ObjId : uint
            {
                OBJID_HSCROLL = 0xFFFFFFFA,
                OBJID_VSCROLL = 0xFFFFFFFB,
                OBJID_CLIENT = 0xFFFFFFFC
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SCROLLBARINFO
            {
                public int cbSize;
                public Rectangle rcScrollBar;
                public int dxyLineButton;
                public int xyThumbTop;
                public int xyThumbBottom;
                public int reserved;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
                public int[] rgstate;
            }

            public enum ScrollInfoMask : uint
            {
                SIF_RANGE = 0x1,
                SIF_PAGE = 0x2,
                SIF_POS = 0x4,
                SIF_DISABLENOSCROLL = 0x8,
                SIF_TRACKPOS = 0x10,
                SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS),
            }
            public enum SBOrientation : int
            {
                SB_HORZ = 0x0,
                SB_VERT = 0x1,
                SB_CTL = 0x2,
                SB_BOTH = 0x3
            }
            [Serializable, StructLayout(LayoutKind.Sequential)]
            public struct SCROLLINFO
            {
                public int cbSize; // (uint) int is because of Marshal.SizeOf
                public uint fMask;
                public int nMin;
                public int nMax;
                public uint nPage;
                public int nPos;
                public int nTrackPos;
            }

            public struct WParam
            {
                public int Value;
                public int Message;
                public WParam(IntPtr wParam)
                {
                    Value = (int)wParam.ToInt64() >> 16;
                    Message = (int)wParam.ToInt64() & 0xFFFF;
                }

                public WParam(int msg, int value)
                {
                    Value = value;
                    Message = msg;
                }

                public int Ptr()
                {
                    return (Value << 16) | Message;
                }
            }
        }
    }
}

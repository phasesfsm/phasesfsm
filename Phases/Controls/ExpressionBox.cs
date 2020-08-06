using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Phases.Controls
{
    public partial class ExpressionBox : TextBox
    {
        private static readonly Size iconSize = new Size(16, 16);
        ImageList imageList;
        Dictionary<string, int> FilteredList;
        bool editing = false;
        List<string> operators;

		private void GeneralInit()
        {
            Dictionary = new Dictionary<string, int>();
            ListBox = new ListBox();
            ListBox.IntegralHeight = false;
            ListBox.KeyUp += ListBox_KeyUp;
            ListBox.DoubleClick += ListBox_DoubleClick;
            ListBox.DrawItem += new DrawItemEventHandler(ListBox_DrawItem);
            ListBox.DrawMode = DrawMode.OwnerDrawFixed;
            ListBox.Visible = false;
            ListBox.TabStop = false;
            ListBox.ItemHeight = iconSize.Height + 2;
            operators = new List<string>();
            operators.Add(" ");
        }

        public ExpressionBox()
        {
            InitializeComponent();
            GeneralInit();
        }

        public ExpressionBox(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            GeneralInit();
            Container.Add(ListBox);
        }

        #region "Properties"

        public Dictionary<string, int> Dictionary { get; set; }

        public ListBox ListBox { get; private set; }

        public ImageList ImageList
        {
            get
            {
                return imageList;
            }
            set
            {
                imageList = value;
                if (imageList != null) imageList.ImageSize = iconSize;
            }
        }

        public int ListBoxMaxItemsCountHeight { get; set; } = 4;

        public List<string> Operators { get => operators; set => operators = value; }

        #endregion

        #region "Utils"

        private string GetCurrentString(out int wordIndex)
        {
            string text = Text;
            wordIndex = SelectionStart;
            if (wordIndex == 0) return "";
            wordIndex--;
            char ch = text[wordIndex];
            while (wordIndex > 0 && !operators.Any(str => str.Last() == ch))
            {
                wordIndex--;
                ch = text[wordIndex];
            }
            if (operators.Any(str => str.Last() == ch)) wordIndex++;
            return text.Substring(wordIndex, SelectionStart - wordIndex);
        }

        private void LocateListBox(bool fullListIfEmpty, bool forceToShow)
        {
            Point cp;
            int index;
            string word = GetCurrentString(out index).ToUpper();
            //if (word == "") return;
            cp = GetPositionFromCharIndex(index == TextLength ? index - 1 : index);
            List<string> lstTemp = new List<string>();
            if (fullListIfEmpty && word == "")
            {
                FilteredList = Dictionary;
            }
            else
            {
                FilteredList = Dictionary.Where(item => item.Key.ToUpper().StartsWith(word)).Select(item => item).OrderBy(key => key.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            int width = 150;
            int x = Math.Min(cp.X + Left - Math.Min((imageList != null ? iconSize.Width + 4 : 0), cp.X + Left - 2), Parent.Width - width - Left - 10);
            int y = cp.Y + Bottom;
            int height;
            if (FilteredList.Count > ListBoxMaxItemsCountHeight)
            {
                height = Math.Min(150, (ListBox.ItemHeight + 1) * ListBoxMaxItemsCountHeight);
                //Parent.Bounds.Height - (cp.Y + Bottom + ListBox.ItemHeight * 2)
            }
            else
            {
                height = (ListBox.ItemHeight + 1) * FilteredList.Count;

            }
            ListBox.SetBounds(x, y, width, height);
            if ((FilteredList.Count != 0 && word != "") || forceToShow)
            {
                ListBox.DataSource = FilteredList.Keys.ToList();
                ListBox.Show();
            }
            else
            {
                ListBox.Hide();
            }
        }

        #endregion

        #region "Override functions"

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (!ListBox.Focused) ListBox.Visible = false;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            ListBox.Parent = Parent;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (editing) return;
            LocateListBox(false, false);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.KeyData)
            {
                case Keys.Enter:
                case Keys.Tab:
                    e.IsInputKey = true;
                    break;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyData)
            {
                case Keys.Up:
                    if (ListBox.SelectedIndex > 0) ListBox.SelectedIndex--;
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Down:
                    if (ListBox.SelectedIndex < ListBox.Items.Count - 1) ListBox.SelectedIndex++;
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Tab:
                    if (ListBox.Visible)
                    {
                        ListBox_SelectOption();
                        e.SuppressKeyPress = true;
                    }
                    break;
                case Keys.Space | Keys.Control:
                    LocateListBox(true, true);
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == 13)
            {
                if (ListBox.Visible == true)
                {
                    ListBox.Focus();
                }
                e.Handled = true;
            }
            else if (e.KeyChar == (char)Keys.Escape)
            {
                ListBox.Visible = false;
                e.Handled = true;
            }
            else if (operators.Any(str => str.Last() == e.KeyChar) && ListBox.Visible)
            {
                ListBox_SelectOption();
            }
        }

        #endregion

        #region "ListBox events"

        private void ListBox_DoubleClick(object sender, EventArgs e)
        {
            ListBox_SelectOption();
        }
		
        private void ListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ListBox_SelectOption();
            }
        }
		
        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (imageList == null)
            {
                e.Graphics.DrawString(ListBox.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds, StringFormat.GenericDefault);
            }
            else
            {
                var image = imageList.Images[FilteredList.Values.ToArray()[e.Index]];
                e.Graphics.DrawImage(image, e.Bounds.X, e.Bounds.Y + e.Bounds.Height / 2 - image.Height / 2, image.Width, image.Height);
                var textSize = e.Graphics.MeasureString(ListBox.Items[e.Index].ToString(), e.Font);
                e.Graphics.DrawString(ListBox.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.X + image.Width + 2, e.Bounds.Y + e.Bounds.Height / 2 - textSize.Height / 2, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();
        }

        private void ListBox_SelectOption()
        {
            if (ListBox.SelectedItem == null) return;
            editing = true;
            int wordIndex;
            string StrLS = GetCurrentString(out wordIndex);
            string text = Text.Remove(wordIndex, SelectionStart - wordIndex);
            text = text.Insert(wordIndex, ListBox.SelectedItem.ToString());
            Text = text;
            SelectionStart = wordIndex + ListBox.SelectedItem.ToString().Length;
            ListBox.Hide();
            Focus();
            editing = false;
        }

        public bool VisibleHelp => ListBox.Visible;

        public void HideHelp()
        {
            ListBox.Visible = false;
        }

        #endregion
    }
}

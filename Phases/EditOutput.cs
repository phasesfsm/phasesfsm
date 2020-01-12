using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Phases.Variables;

namespace Phases
{
    public partial class EditOutput : Form
    {
        private struct IndexOption
        {
            public int Index;
            public int Option;
            public IndexOption(int index, int option)
            {
                Index = index;
                Option = option;
            }
        }

        Dictionary<string, int> dictionary;
        Dictionary<string, IndexOption> result;
        string value;

        public EditOutput(Dictionary<string, int> dictionary, string value)
        {
            InitializeComponent();
            this.dictionary = dictionary;
            this.value = value;
        }

        private void EditOutput_Load(object sender, EventArgs e)
        {
            result = new Dictionary<string, IndexOption>();
            string[] values = value.Split(new char[] { ',', ' ' });
            string key;
            foreach (string str in values)
            {
                if (str != "")
                {
                    key = GetKeyAndIndexOption(str, out IndexOption indexOption);
                    if (key != "")
                    {
                        result.Add(key, indexOption);
                        dictionary.Remove(key);
                    }
                }
            }
            listAll.DataSource = dictionary.Keys.ToList();
            listOut.DataSource = result.Keys.ToList();
            listOut.Focus();
        }

        private void listAll_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (imageList == null)
            {
                e.Graphics.DrawString(listAll.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds, StringFormat.GenericDefault);
            }
            else if(e.Index >= 0)
            {
                var image = imageList.Images[dictionary.Values.ToArray()[e.Index]];
                e.Graphics.DrawImage(image, e.Bounds.X, e.Bounds.Y + e.Bounds.Height / 2 - image.Height / 2, image.Width, image.Height);
                var textSize = e.Graphics.MeasureString(listAll.Items[e.Index].ToString(), e.Font);
                e.Graphics.DrawString(listAll.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.X + image.Width + 2, e.Bounds.Y + e.Bounds.Height / 2 - textSize.Height / 2, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();
        }

        private void listOut_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (imageList == null)
            {
                e.Graphics.DrawString(listOut.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds, StringFormat.GenericDefault);
            }
            else if (e.Index >= 0)
            {
                var image = imageList.Images[result.Values.ToArray()[e.Index].Index];
                e.Graphics.DrawImage(image, e.Bounds.X, e.Bounds.Y + e.Bounds.Height / 2 - image.Height / 2, image.Width, image.Height);
                var textSize = e.Graphics.MeasureString(listOut.Items[e.Index].ToString(), e.Font);
                e.Graphics.DrawString(GetIndexOptionResult(e.Index), e.Font, new SolidBrush(e.ForeColor), e.Bounds.X + image.Width + 8, e.Bounds.Y + e.Bounds.Height / 2 - textSize.Height / 2, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            StringBuilder res = new StringBuilder();
            
            foreach(string str in result.Keys)
            {
                if (res.Length == 0) res.Append(GetIndexOptionResult(str));
                else res.Append("," + GetIndexOptionResult(str));
            }
            Tag = res.ToString();
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            if (listAll.SelectedItems.Count == 0) return;
            var str = listAll.SelectedItem as string;
            int index, sel = listAll.SelectedIndex;
            dictionary.TryGetValue(str, out index);
            result.Add(str, GetDefaultOption(index));
            dictionary.Remove(str);
            listAll.DataSource = dictionary.Keys.ToList();
            listOut.DataSource = result.Keys.ToList();
            if (sel < listAll.Items.Count) listAll.SelectedIndex = sel;
            else if (listAll.Items.Count != 0) listAll.SelectedIndex = listAll.Items.Count - 1;
        }

        private void btRemove_Click(object sender, EventArgs e)
        {
            if (listOut.SelectedItems.Count == 0) return;
            var str = listOut.SelectedItem as string;
            IndexOption indexOption;
            int sel = listOut.SelectedIndex;
            result.TryGetValue(str, out indexOption);
            dictionary.Add(str, indexOption.Index);
            result.Remove(str);
            listAll.DataSource = dictionary.Keys.ToList();
            listOut.DataSource = result.Keys.ToList();
            if (sel < listOut.Items.Count) listOut.SelectedIndex = sel;
            else if (listOut.Items.Count != 0) listOut.SelectedIndex = listOut.Items.Count - 1;
        }

        private void listAll_DoubleClick(object sender, EventArgs e)
        {
            btAdd_Click(listAll, e);
        }

        private void listOut_DoubleClick(object sender, EventArgs e)
        {
            btRemove_Click(listOut, e);
        }

        private void SetOptions(bool send = false, bool changeTrue = false, bool changeFalse = false, bool toggle = false, bool clear = false, bool max = false, bool min = false, bool inc = false, bool dec = false)
        {
            radioSend.Enabled = send;
            radioTrue.Enabled = changeTrue;
            radioFalse.Enabled = changeFalse;
            radioToggle.Enabled = toggle;
            radioClear.Enabled = clear;
            radioMax.Enabled = max;
            radioMin.Enabled = min;
            radioIncrement.Enabled = inc;
            radioDecrement.Enabled = dec;
        }

        private void CheckOption(int option)
        {
            if (option == -1) return;
            RadioButton[] rads = new RadioButton[] { radioSend, radioTrue, radioFalse, radioToggle, radioClear, radioMax, radioMin, radioIncrement, radioDecrement };
            rads[option].Checked = true;
        }

        private void Options_Click(object sender, EventArgs e)
        {
            RadioButton[] rads = new RadioButton[] { radioSend, radioTrue, radioFalse, radioToggle, radioClear, radioMax, radioMin, radioIncrement, radioDecrement };
            int option = 0;
            foreach (RadioButton rad in rads)
            {
                if (rad.Checked) break;
                option++;
            }
            if (listOut.SelectedItems.Count == 0) return;
            if (result.TryGetValue(listOut.SelectedItem as string, out IndexOption indexOption))
            {
                indexOption.Option = option;
                result[listOut.SelectedItem as string] = indexOption;
            }
            listOut.Refresh();
        }

        private string GetIndexOptionResult(string key)
        {
            StringBuilder str = new StringBuilder(key);
            if (result.TryGetValue(key, out IndexOption indexOption))
            {
                switch (indexOption.Index)
                {
                    case VariableCollection.ImageIndex.EventOutput:
                    case VariableCollection.ImageIndex.MessageFlag:
                        if (indexOption.Option == 0) str.Insert(0, '»');
                        break;
                    case VariableCollection.ImageIndex.BooleanOutput:
                    case VariableCollection.ImageIndex.BooleanFlag:
                        if (indexOption.Option == 2) str.Insert(0, '!');
                        if (indexOption.Option == 3) str.Insert(0, '~');
                        break;
                    case VariableCollection.ImageIndex.CounterFlag:
                        if (indexOption.Option == 4) str.Insert(0, '!');
                        if (indexOption.Option == 5) str.Insert(0, '\'');
                        if (indexOption.Option == 6) str.Insert(0, '.');
                        if (indexOption.Option == 7) str.Append('+');
                        if (indexOption.Option == 8) str.Append('-');
                        break;
                }
            }
            return str.ToString();
        }

        private string GetIndexOptionResult(int index)
        {
            return GetIndexOptionResult(result.Keys.ToArray()[index]);
        }

        private int GetIndex(string key)
        {
            if(dictionary.TryGetValue(key, out int index))
            {
                return index;
            }
            return -1;
        }

        private string GetKeyAndIndexOption(string value, out IndexOption indexOption)
        {
            StringBuilder str = new StringBuilder(value);
            int option = -1, index;
            if (value.First() == '!')
            {
                str.Remove(0, 1);
                index = GetIndex(str.ToString());
                switch (index)
                {
                    case VariableCollection.ImageIndex.BooleanOutput:
                    case VariableCollection.ImageIndex.BooleanFlag:
                        option = 2;
                        break;
                }
            }
            else if (value.First() == '~')
            {
                str.Remove(0, 1);
                index = GetIndex(str.ToString());
                switch (index)
                {
                    case VariableCollection.ImageIndex.BooleanOutput:
                    case VariableCollection.ImageIndex.BooleanFlag:
                        option = 3;
                        break;
                }
            }
            else if (value.First() == '»')
            {
                str.Remove(0, 1);
                index = GetIndex(str.ToString());
                switch (index)
                {
                    case VariableCollection.ImageIndex.EventOutput:
                    case VariableCollection.ImageIndex.MessageFlag:
                        option = 0;
                        break;
                }
            }
            else if(value.First() == '\'')
            {
                str.Remove(0, 1);
                index = GetIndex(str.ToString());
                switch (index)
                {
                    case VariableCollection.ImageIndex.CounterFlag:
                        option = 5;
                        break;
                }
            }
            else if (value.First() == '.')
            {
                str.Remove(0, 1);
                index = GetIndex(str.ToString());
                switch (index)
                {
                    case VariableCollection.ImageIndex.CounterFlag:
                        option = 6;
                        break;
                }
            }
            else if (value.Last() == '+')
            {
                str.Remove(str.Length - 1, 1);
                index = GetIndex(str.ToString());
                switch (index)
                {
                    case VariableCollection.ImageIndex.CounterFlag:
                        option = 7;
                        break;
                }
            }
            else if (value.Last() == '-')
            {
                str.Remove(str.Length - 1, 1);
                index = GetIndex(str.ToString());
                switch (index)
                {
                    case VariableCollection.ImageIndex.CounterFlag:
                        option = 8;
                        break;
                }
            }
            else
            {
                index = GetIndex(str.ToString());
                switch (index)
                {
                    case VariableCollection.ImageIndex.BooleanFlag:
                    case VariableCollection.ImageIndex.BooleanOutput:
                        option = 1;
                        break;
                }
            }
            indexOption = new IndexOption(index, option);
            if (index == -1) return "";
            return str.ToString();
        }

        private IndexOption GetDefaultOption(int index)
        {
            int option = -1;
            switch (index)
            {
                case VariableCollection.ImageIndex.BooleanOutput:
                    option = 1;
                    break;
                case VariableCollection.ImageIndex.EventOutput:
                case VariableCollection.ImageIndex.MessageFlag:
                    option = 0;
                    break;
                case VariableCollection.ImageIndex.BooleanFlag:
                    option = 1;
                    break;
                case VariableCollection.ImageIndex.CounterFlag:
                    option = 4;
                    break;
            }
            return new IndexOption(index, option);
        }

        private void listOut_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listOut.SelectedItems.Count == 0) return;
            IndexOption indexOption;
            if (result.TryGetValue(listOut.SelectedItem as string, out indexOption))
            {
                switch (indexOption.Index)
                {
                    case VariableCollection.ImageIndex.BooleanFlag:
                    case VariableCollection.ImageIndex.BooleanOutput:
                        SetOptions(false, true, true, true);
                        break;
                    case VariableCollection.ImageIndex.EventOutput:
                    case VariableCollection.ImageIndex.MessageFlag:
                        SetOptions(true);
                        break;
                    case VariableCollection.ImageIndex.CounterFlag:
                        SetOptions(false, false, false, false, true, true, true, true, true);
                        break;
                }
                CheckOption(indexOption.Option);
            }
        }
    }
}

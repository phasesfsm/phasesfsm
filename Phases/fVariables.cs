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
    public partial class fVariables : Form
    {
        private VariableCollection Variables;

        public fVariables(object variables)
        {
            if (!(variables is VariableCollection)) throw new Exception("Objeto invalido.");
            Variables = (VariableCollection)variables;
            InitializeComponent();
        }

        private void fVariables_Load(object sender, EventArgs e)
        {
            foreach(Input var in Variables.Inputs)
            {
                inputsList.Items.Add(var.Item);
                if(var is BooleanInput) inputsList.Groups["grInputBooleans"].Items.Add(var.Item);
                else if (var is EventInput) inputsList.Groups["grInputEvents"].Items.Add(var.Item);
            }
            foreach (Output var in Variables.Outputs)
            {
                outputsList.Items.Add(var.Item);
                if (var is BooleanOutput) outputsList.Groups["grOutputBooleans"].Items.Add(var.Item);
                else if (var is EventOutput) outputsList.Groups["grOutputEvents"].Items.Add(var.Item);
            }
            foreach (Flag var in Variables.Flags)
            {
                flagsList.Items.Add(var.Item);
                if (var is BooleanFlag) flagsList.Groups["grFlagBooleans"].Items.Add(var.Item);
                else if (var is CounterFlag) flagsList.Groups["grFlagCounters"].Items.Add(var.Item);
                else if (var is MessageFlag) flagsList.Groups["grFlagMessages"].Items.Add(var.Item);
            }
        }

        private void fVariables_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach(Variable var in Variables.All)
            {
                var.Item.Remove();
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedItems.Count == 0) propertyGrid.SelectedObject = null;
            else propertyGrid.SelectedObject = listView.SelectedItems[0].Tag;
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            propertyGrid.Refresh();
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabInputs)
            {
                listView_SelectedIndexChanged(inputsList, null);
            }
            else if (tabControl.SelectedTab == tabOutputs)
            {
                listView_SelectedIndexChanged(outputsList, null);
            }
            else if (tabControl.SelectedTab == tabFlags)
            {
                listView_SelectedIndexChanged(flagsList, null);
            }
        }

        private void btAddInputBoolean_Click(object sender, EventArgs e)
        {
            Variable variable = Variables.AddVariable<BooleanInput>();
            inputsList.Items.Add(variable.Item);
            inputsList.Groups["grInputBooleans"].Items.Add(variable.Item);
            variable.Item.Selected = true;
        }

        private void btAddInputEvent_Click(object sender, EventArgs e)
        {
            Variable variable = Variables.AddVariable<EventInput>();
            inputsList.Items.Add(variable.Item);
            inputsList.Groups["grInputEvents"].Items.Add(variable.Item);
            variable.Item.Selected = true;
        }

        private void btAddOutputBoolean_Click(object sender, EventArgs e)
        {
            Variable variable = Variables.AddVariable<BooleanOutput>();
            outputsList.Items.Add(variable.Item);
            outputsList.Groups["grOutputBooleans"].Items.Add(variable.Item);
            variable.Item.Selected = true;
        }

        private void btAddOutputEvent_Click(object sender, EventArgs e)
        {
            Variable variable = Variables.AddVariable<EventOutput>();
            outputsList.Items.Add(variable.Item);
            outputsList.Groups["grOutputEvents"].Items.Add(variable.Item);
            variable.Item.Selected = true;
        }

        private void btAddFlagBoolean_Click(object sender, EventArgs e)
        {
            Variable variable = Variables.AddVariable<BooleanFlag>();
            flagsList.Items.Add(variable.Item);
            flagsList.Groups["grFlagBooleans"].Items.Add(variable.Item);
            variable.Item.Selected = true;
        }

        private void btAddFlagCounter_Click(object sender, EventArgs e)
        {
            Variable variable = Variables.AddVariable<CounterFlag>();
            flagsList.Items.Add(variable.Item);
            flagsList.Groups["grFlagCounters"].Items.Add(variable.Item);
            variable.Item.Selected = true;
        }

        private void btAddFlagMessage_Click(object sender, EventArgs e)
        {
            Variable variable = Variables.AddVariable<MessageFlag>();
            flagsList.Items.Add(variable.Item);
            flagsList.Groups["grFlagMessages"].Items.Add(variable.Item);
            variable.Item.Selected = true;
        }

        private void btDeleteInput_Click(object sender, EventArgs e)
        {
            if (inputsList.SelectedIndices.Count == 0) return;
            var item = inputsList.SelectedItems[0];
            var variable = (Variable)item.Tag;
            inputsList.Items.Remove(item);
            Variables.RemoveVariable(variable);
        }

        private void btDeleteOutput_Click(object sender, EventArgs e)
        {
            if (outputsList.SelectedIndices.Count == 0) return;
            var item = outputsList.SelectedItems[0];
            var variable = (Variable)item.Tag;
            outputsList.Items.Remove(item);
            Variables.RemoveVariable(variable);
        }

        private void btDeleteFlag_Click(object sender, EventArgs e)
        {
            if (flagsList.SelectedIndices.Count == 0) return;
            var item = flagsList.SelectedItems[0];
            var variable = (Variable)item.Tag;
            flagsList.Items.Remove(item);
            Variables.RemoveVariable(variable);
        }
    }
}

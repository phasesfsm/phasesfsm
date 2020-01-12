using Phases.Actions;
using Phases.DrawableObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Phases
{
    public partial class ActionsViewer : Form
    {
        private Matrix ShadowTransform = new Matrix();
        private float ShadowScale = 0.4f;
        private DrawAction activeAction = null;
        private List<RecordableAction> actionsList;

        internal ActionsViewer(List<RecordableAction> list)
        {
            actionsList = list;
            InitializeComponent();
        }

        private void ActionsViewer_Load(object sender, EventArgs e)
        {
            ShadowTransform.Scale(ShadowScale, ShadowScale);
            fDraw.SetViewToPosition(pDrawRef, Point.Empty, ShadowTransform);
            fDraw.SetViewToPosition(pShadow, Point.Empty, ShadowTransform);
            fDraw.SetViewToPosition(pAfterAction, Point.Empty, ShadowTransform);
            RefreshActions();
        }

        public void RefreshActions()
        {
            dgActions.Rows.Clear();
            foreach (RecordableAction action in actionsList)
            {
                if (action is DrawAction daction)
                {
                    dgActions.Rows.Add(daction.ActionType.ToString(), daction.ShadowState.Count, daction.AfterAction.Count, daction.DrawRef.Count,
                        daction.Selection.Count, daction.FocusSelectionIndex);
                }
                else
                {
                    dgActions.Rows.Add(action.ActionType.ToString(), "", "", "");
                }
            }
        }

        private void pDrawRef_Paint(object sender, PaintEventArgs e)
        {
            if (activeAction == null) return;
            
            //Scaling
            e.Graphics.Transform = ShadowTransform;

            //Drawing objects
            foreach (DrawableObject obj in activeAction.DrawRef)
            {
                obj.Draw(e.Graphics, ShadowScale);
            }
        }

        private void pShadow_Paint(object sender, PaintEventArgs e)
        {
            if (activeAction == null) return;

            //Scaling
            e.Graphics.Transform = ShadowTransform;

            //Drawing objects
            foreach (DrawableObject obj in activeAction.ShadowState)
            {
                obj.Draw(e.Graphics, ShadowScale);
            }
        }

        private void pAfterAction_Paint(object sender, PaintEventArgs e)
        {
            if (activeAction == null) return;

            //Scaling
            e.Graphics.Transform = ShadowTransform;

            //Drawing objects
            foreach (DrawableObject obj in activeAction.AfterAction)
            {
                obj.Draw(e.Graphics, ShadowScale);
            }
        }

        private void dgActions_SelectionChanged(object sender, EventArgs e)
        {
            if (dgActions.SelectedCells.Count == 0) return;
            int index = dgActions.SelectedCells[0].RowIndex;
            if (actionsList[index] is DrawAction daction)
            {
                activeAction = daction;
                dgObjects.Rows.Clear();
                switch (dgActions.SelectedCells[0].ColumnIndex)
                {
                    case 1:
                        foreach (DrawableObject obj in activeAction.ShadowState)
                        {
                            dgObjects.Rows.Add(obj.Name, obj.ObjNumber, obj.zInstance);
                        }
                        break;
                    case 2:
                        foreach (DrawableObject obj in activeAction.AfterAction)
                        {
                            dgObjects.Rows.Add(obj.Name, obj.ObjNumber, obj.zInstance);
                        }
                        break;
                    case 4:
                    case 5:
                        foreach (DrawableObject obj in activeAction.Selection)
                        {
                            dgObjects.Rows.Add(obj.Name, obj.ObjNumber, obj.zInstance);
                        }
                        break;
                    default:
                        foreach (DrawableObject obj in activeAction.DrawRef)
                        {
                            dgObjects.Rows.Add(obj.Name, obj.ObjNumber, obj.zInstance);
                        }
                        break;
                }
            }
            else
            {
                activeAction = null;
                dgObjects.Rows.Clear();
            }
            pDrawRef.Refresh();
            pShadow.Refresh();
            pAfterAction.Refresh();
        }

        private void dgObjects_SelectionChanged(object sender, EventArgs e)
        {
            if (dgObjects.SelectedRows.Count == 0) return;
            int index = dgObjects.SelectedCells[0].RowIndex;
            switch (dgActions.SelectedCells[0].ColumnIndex)
            {
                case 1:
                    propertyGrid1.SelectedObject = activeAction.ShadowState[index];
                    break;
                case 2:
                    propertyGrid1.SelectedObject = activeAction.AfterAction[index];
                    break;
                case 4:
                case 5:
                    propertyGrid1.SelectedObject = activeAction.Selection[index];
                    break;
                default:
                    propertyGrid1.SelectedObject = activeAction.DrawRef[index];
                    break;
            }
        }
    }
}

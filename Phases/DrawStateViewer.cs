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
    public partial class DrawStateViewer : Form
    {
        private Matrix ShadowTransform = new Matrix();
        private float ShadowScale = 0.4f;
        PhasesBook book;
        MouseTool mouse;

        internal DrawStateViewer(PhasesBook book, MouseTool mouse)
        {
            this.book = book;
            this.mouse = mouse;
            InitializeComponent();
        }

        private void DrawStateViewer_Load(object sender, EventArgs e)
        {
            ShadowTransform.Scale(ShadowScale, ShadowScale);
            fDraw.SetViewToPosition(pShadow, Point.Empty, ShadowTransform);
            fDraw.SetViewToPosition(pDraw, Point.Empty, ShadowTransform);
            fDraw.SetViewToPosition(pSelection, Point.Empty, ShadowTransform);

            foreach (DrawableObject obj in book.SelectedSheet.draw.Shadow)
            {
                dgShadow.Rows.Add(obj.Name, obj.ObjNumber, obj.zInstance);
            }
            foreach (DrawableObject obj in book.SelectedSheet.draw.Objects)
            {
                dgDraw.Rows.Add(obj.Name, obj.ObjNumber, obj.zInstance);
            }
            foreach (DrawableObject obj in mouse.SelectedObjects)
            {
                dgSelection.Rows.Add(obj.Name, obj.ObjNumber, obj.zInstance);
            }
        }

        private void pShadow_Paint(object sender, PaintEventArgs e)
        {
            //Scaling
            e.Graphics.Transform = ShadowTransform;

            //Drawing objects
            foreach (DrawableObject obj in book.SelectedSheet.draw.Shadow)
            {
                obj.Draw(e.Graphics, ShadowScale);
            }
        }

        private void pDraw_Paint(object sender, PaintEventArgs e)
        {
            //Scaling
            e.Graphics.Transform = ShadowTransform;

            //Drawing objects
            foreach (DrawableObject obj in book.SelectedSheet.draw.Objects)
            {
                obj.Draw(e.Graphics, ShadowScale);
            }
        }

        private void pSelection_Paint(object sender, PaintEventArgs e)
        {
            //Scaling
            e.Graphics.Transform = ShadowTransform;

            //Drawing objects
            foreach (DrawableObject obj in mouse.SelectedObjects)
            {
                obj.Draw(e.Graphics, ShadowScale);
            }
        }

        private void dgShadow_Click(object sender, EventArgs e)
        {
            dgDraw.ClearSelection();
            dgSelection.ClearSelection();
            if (dgShadow.SelectedRows.Count == 0) return;
            propertyGrid1.SelectedObject = book.SelectedSheet.draw.Shadow[dgShadow.SelectedRows[0].Index];
        }

        private void dgDraw_Click(object sender, EventArgs e)
        {
            dgShadow.ClearSelection();
            dgSelection.ClearSelection();
            if (dgDraw.SelectedRows.Count == 0) return;
            propertyGrid1.SelectedObject = book.SelectedSheet.draw.Objects[dgDraw.SelectedRows[0].Index];
        }

        private void dgSelection_Click(object sender, EventArgs e)
        {
            dgShadow.ClearSelection();
            dgDraw.ClearSelection();
            if (dgSelection.SelectedRows.Count == 0) return;
            propertyGrid1.SelectedObject = mouse.SelectedObjects[dgSelection.SelectedRows[0].Index];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing;

namespace Direct3DLib
{
	public class Direct3DDesigner : ControlDesigner
	{

		private bool enableContextMenu = true;
		private bool enableControlDrag = true;
		private Direct3DControl d3dControl;

		public Direct3DDesigner()
			: base()
		{
		}

		protected override void OnContextMenu(int x, int y)
		{
			if(enableContextMenu)
				base.OnContextMenu(x, y);
		}

		protected override void OnCreateHandle()
		{
			base.OnCreateHandle();
			if (Control != null)
			{
				d3dControl = Control as Direct3DControl;
				Control.Paint += new PaintEventHandler(Control_Paint);
			}
		}
		void Control_Paint(object sender, PaintEventArgs e)
		{
			d3dControl.Render();
		}

		[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
		protected override void WndProc(ref Message m)
		{
			if (m.Msg >= WndProcMessage.MIN_MOUSE && m.Msg <= WndProcMessage.MAX_MOUSE)
			{
				if (d3dControl != null)
					d3dControl.ProcessDesignTimeMouseMessage(ref m);
			}
			base.WndProc(ref m);
		}


		protected override void OnMouseDragMove(int x, int y)
		{
			if(enableControlDrag)
				base.OnMouseDragMove(x, y);
		}
		 
	}
}

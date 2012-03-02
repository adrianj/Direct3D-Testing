using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using D3DControl = Direct3DExtensions.D3DHostControl;

namespace Direct3DExtensions
{
	public partial class D3DHostForm : Form
	{
		protected D3DControl direct3DControl;
		PropertyGrid grid = new PropertyGrid();
		Form gridForm = new Form();
		bool preventClose = true;

		//public Direct3DEngine D3DEngine { get { return direct3DControl.D3DEngine; } }

		public D3DHostForm()
		{
			InitializeComponent();
			grid.Dock = DockStyle.Fill;
			gridForm.Controls.Add(grid);
			gridForm.Text = "D3DControl Properties";
			gridForm.Size = new System.Drawing.Size(500, 500);
			this.FormClosing += (o,e) => { preventClose = false;};
			gridForm.FormClosing += (o, e) => { e.Cancel = preventClose; gridForm.Hide(); };
		}

		void SelectOrCreateD3DControl()
		{
			foreach (Control con in this.Controls)
			{
				if (con is D3DHostControl)
				{
					direct3DControl = con as D3DControl;
					break;
				}
			}
			if (direct3DControl == null)
			{
				this.direct3DControl = new D3DHostControl();
				this.direct3DControl.Dock = System.Windows.Forms.DockStyle.Fill;
				this.direct3DControl.Location = new System.Drawing.Point(0, 0);
				this.direct3DControl.TabIndex = 0;
				this.Controls.Add(this.direct3DControl);
			}
		}

		public void SetEngine(Direct3DEngine engine)
		{
			SelectOrCreateD3DControl();
			engine.HostControl = direct3DControl;
			grid.SelectedObject = direct3DControl.D3DEngine;

			// Immediately starts engine on form show.
			this.Shown += (o, e) => { this.StartDirect3D(); };

			this.PopoutPropertyGrid();
		}

		public void StartDirect3D()
		{
			direct3DControl.InitializeDirect3D();
		}

		void PopoutPropertyGrid()
		{
			gridForm.Show();

		}

		public void CloseByExternalThread()
		{
			if (this.InvokeRequired)
			{
				if(!this.IsDisposed)
					this.Invoke(new Action(CloseByExternalThread));
			}
			Console.WriteLine("Close request");
			this.Close();
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using D3DControl = Direct3DExtensions.Basic3DControl;

namespace Direct3DExtensions
{
	public partial class HostForm : Form
	{
		protected D3DControl direct3DControl;
		PropertyGrid grid = new PropertyGrid();
		Form gridForm = new Form();

		public HostForm()
		{
			InitializeComponent();
			grid.Dock = DockStyle.Fill;
			gridForm.Controls.Add(grid);
			gridForm.Text = "D3DControl Properties";
			gridForm.Size = new System.Drawing.Size(640, 480);
			gridForm.FormClosing += (o, e) => { e.Cancel = true; gridForm.Hide(); };
		}

		public void SetControl(D3DControl control)
		{
			if (components != null)
				components.Dispose();
			else if (direct3DControl != null)
			{
				this.Controls.Remove(direct3DControl);
				direct3DControl.Dispose();
			}

			this.direct3DControl = control;
			this.SuspendLayout();
			// 
			// direct3DControl
			// 
			this.direct3DControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.direct3DControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.direct3DControl.Location = new System.Drawing.Point(0, 0);
			this.direct3DControl.TabIndex = 0;
			this.Controls.Add(this.direct3DControl);
			this.ResumeLayout(false);

			grid.SelectedObject = direct3DControl;

			this.Shown += (o, e) => { this.StartDirect3D(); };
			this.PopoutPropertyGrid();
		}


		private void HostForm_Load(object sender, EventArgs e)
		{
			if (direct3DControl == null)
			{
				D3DControl con = new D3DControl();
				con.BackColor = Color.LightBlue;
				SetControl(con);
			}
			
		}

		public void StartDirect3D()
		{
			direct3DControl.InitializeDirect3D();
		}

		void PopoutPropertyGrid()
		{
			gridForm.Show();

		}
	}
}

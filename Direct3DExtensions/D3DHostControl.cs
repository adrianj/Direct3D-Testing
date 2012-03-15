using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;

namespace Direct3DExtensions
{
	public class D3DHostControl : SelectableControl
	{
		public bool IsDesignMode { get; private set; }

		Label errorLabel;
		Direct3DEngine eng;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Direct3DEngine D3DEngine { get { return eng; } set { eng = value; } }

		public D3DHostControl()
			: base()
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				IsDesignMode = true;
			this.BackColor = System.Drawing.Color.LightBlue;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			errorLabel = new Label();
			errorLabel.Dock = DockStyle.Fill;
			errorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			errorLabel.Text = "If you can read this, something went wrong.  Or we're in design mode."+
			"\n\nPossibility 1:  You need to call this.InitializeDirect3D()."+
			"\n\nPossibility 2:  You haven't added a Direct3DEngine implementation, and set its HostControl as this."+
			"\n\nPossibility 3:  The Direct3D Engine threw an uncaught exception.";
			this.Controls.Add(errorLabel);
		}

		public void InitializeDirect3D()
		{
			if (IsDesignMode) return;
			try
			{
				D3DEngine.InitializeDirect3D();
				errorLabel.Visible = false;
			}
			catch (System.IO.IOException ex)
			{
				errorLabel.Text = "Direct3D Initialization Error\n\n" + ex;
				errorLabel.Visible = true;
			}
		}

		public void SetEngine(Direct3DEngine engine)
		{
			if (engine == null) return;
			this.D3DEngine = engine;
		}


		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		bool disposed = false;
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (!disposed)
				{
					DisposeManaged();
				}
				disposed = true;
			}
			base.Dispose(disposing);
		}

		private void DisposeManaged()
		{
			if (D3DEngine != null) D3DEngine.Dispose();
			D3DEngine = null;
		}
	}
}

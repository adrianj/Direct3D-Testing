﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace Direct3DExtensions
{

	public class SelectableControl : UserControl
	{
		public SelectableControl()
		{
			this.SetStyle(ControlStyles.Selectable, true);
			this.TabStop = true;
			this.Load += (o, e) => { if (this.Parent != null) this.Parent.MouseWheel += OnParentMouseWheel; };
		}

		public void OnParentMouseWheel(object sender, MouseEventArgs e)
		{
			if(this.Focused)
				this.OnMouseWheel(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			this.Focus();
			base.OnMouseDown(e);
		}
		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Up || keyData == Keys.Down) return true;
			if (keyData == Keys.Left || keyData == Keys.Right) return true;
			return base.IsInputKey(keyData);
		}
		protected override void OnEnter(EventArgs e)
		{
			this.Invalidate();
			base.OnEnter(e);
		}
		protected override void OnLeave(EventArgs e)
		{
			this.Invalidate();
			base.OnLeave(e);
		}
		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);
			if (this.Focused)
			{
				var rc = this.ClientRectangle;
				rc.Inflate(-2, -2);
				ControlPaint.DrawFocusRectangle(pe.Graphics, rc);
			}
		}
	}
}


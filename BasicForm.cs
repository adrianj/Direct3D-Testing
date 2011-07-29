using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Direct3DLib
{
	public partial class BasicForm : Direct3DForm
	{
		public BasicForm()
		{
			InitializeComponent();
			Shape cube = new PictureTile();
			direct3DControl1.Engine.ShapeList.Add(cube);
			//direct3DControl1.Engine.UpdateShapes();
		}

		public override void Render()
		{
			textBox1.Text = "" + direct3DControl1.Engine.Camera;
			direct3DControl1.Render();
		}
		 
	}
}

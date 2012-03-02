using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Direct3DExtensions_Test
{
	public partial class TestForm : Form
	{
		public TestForm()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			d3DHostControl1.InitializeDirect3D();
		}

		private void independentMeshEngine1_CameraChanged(object sender, EventArgs e)
		{

		}
	}
}

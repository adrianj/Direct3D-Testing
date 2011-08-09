using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Direct3DLib;
using SlimDX;

namespace Direct3DLib_Test
{
    public partial class TestForm : Direct3DForm
    {
        public TestForm()
        {
            InitializeComponent();
            foreach (Direct3DControl.MouseOption mo in Enum.GetValues(typeof(Direct3DControl.MouseOption)))
            {
                comboBox1.Items.Add(mo);
                comboBox2.Items.Add(mo);
                comboBox3.Items.Add(mo);
            }
            comboBox1.SelectedItem = earth3DControl.LeftMouseFunction;
			comboBox2.SelectedItem = earth3DControl.RightMouseFunction;
			comboBox3.SelectedItem = earth3DControl.BothMouseFunction;
                InitTest();
          
        }

        private void InitTest()
		{
			earth3DControl.SelectedObjectChanged += (o, e) =>
            {
				object obj = earth3DControl.SelectedObject;
				if (obj is Shape)
				{
					Shape s = obj as Shape;
					propertyGrid.SelectedObject = s;
					textBox1.Text = "" + s;
				}
            };
			propertyGrid.SelectedObject = earth3DControl;
			//AddHerc();
			//AddSphere();
			//AddWoomera();
			earth3DControl.UpdateShapes();
        }




		private Shape OpenShapeFromFile(string filename)
		{
			Shape shape = null;
			try
			{
				shape = ComplexShapeFactory.CreateFromFile(filename);
			}
			catch (System.IO.IOException iox)
			{
				MessageBox.Show("Create From File: " + filename + " failed\n\n"+iox);
			}
			return shape;
		}

        private void PreRenderTest()
        {
			textBox1.Text = earth3DControl.debugString;
			textBox2.Text = "Refresh Rate: " + String.Format("{0:#00}", earth3DControl.RefreshRate)
				+ "\tLightDir: " + earth3DControl.LightDirection
				+ "\tCamTarget: " + earth3DControl.CameraLocation
				+ "\tCamTilt: " + earth3DControl.CameraTilt
				+ "\tCamPan: " + earth3DControl.CameraPan
				+ "\nControl Status: "+earth3DControl.Visible;
        }

        public override void Render()
        {
            PreRenderTest();
			earth3DControl.Render();
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            Control con = sender as Control;
            if (con.Focused)
            {
                if (sender == comboBox1)
					earth3DControl.LeftMouseFunction = (Direct3DControl.MouseOption)comboBox1.SelectedItem;
                if (sender == comboBox2)
					earth3DControl.RightMouseFunction = (Direct3DControl.MouseOption)comboBox2.SelectedItem;
                if (sender == comboBox3)
					earth3DControl.BothMouseFunction = (Direct3DControl.MouseOption)comboBox3.SelectedItem;
            }
        }


		private void button2_Click(object sender, EventArgs e)
		{
		}


    }
}

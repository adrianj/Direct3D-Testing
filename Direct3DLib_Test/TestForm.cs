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

		Float3 loc = new Float3();
        private void PreRenderTest()
        {
			//loc.X += 10;
			if (loc.X > 1.75e7)
				loc.X = 1.74809e7f;
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

		private void earth3DControl_Load(object sender, EventArgs e)
		{
			loc.Y = 0;

		}

		private void TestForm_Load(object sender, EventArgs e)
		{
			InitTest();
		}





    }
}

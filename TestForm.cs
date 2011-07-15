using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SlimDX;

namespace Direct3DLib
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
            comboBox1.SelectedItem = direct3DControl.LeftMouseFunction;
            comboBox2.SelectedItem = direct3DControl.RightMouseFunction;
            comboBox3.SelectedItem = direct3DControl.BothMouseFunction;
            
            if (direct3DControl.IsInitialized)
            {
                InitTest();
            }
        }


        private void InitTest()
        {
            direct3DControl.SelectedObjectChanged += (o, e) =>
            {
				object obj = direct3DControl.SelectedObject;
				if (obj is Shape)
				{
					Shape s = obj as Shape;
					propertyGrid.SelectedObject = direct3DControl.SelectedObject;
					textBox1.Text = "" + s;
				}
            };
			Shape shape = ComplexShape.CreateFromFile("C:\\Users\\adrianj\\Documents\\Work\\CAD\\hercules_LORES.stl");
			if (shape != null)
			{
				shape.Rotation = new Vector3(-(float)Math.PI / 2,0,0);
				shape.Location = new Vector3(0, 0, 5);
				direct3DControl.Engine.ShapeList.Add(shape);
			}


			Sphere sp = new Sphere(12,Color.Red);
			sp.LongLines = 6;
			sp.LatLines = 7;
			sp.Location = new Vector3(0, 0, 0);
			sp.Scale = new Vector3(30, 30, 30);
			sp.Topology = SlimDX.Direct3D10.PrimitiveTopology.LineStrip;
			direct3DControl.Engine.ShapeList.Add(sp);
			
			direct3DControl.Engine.UpdateShapes();
             
        }



        private void PreRenderTest()
        {

			textBox2.Text = "Refresh Rate: " + String.Format("{0:000.00}",direct3DControl.Engine.RefreshRate)
				+ "\t LightDir: "+direct3DControl.Engine.LightDirection;
        }

        public override void Render()
        {
            PreRenderTest();
            if(direct3DControl.IsInitialized)
                direct3DControl.Render();
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            Control con = sender as Control;
            if (con.Focused)
            {
                if (sender == comboBox1)
                    direct3DControl.LeftMouseFunction = (Direct3DControl.MouseOption)comboBox1.SelectedItem;
                if (sender == comboBox2)
                    direct3DControl.RightMouseFunction = (Direct3DControl.MouseOption)comboBox2.SelectedItem;
                if (sender == comboBox3)
                    direct3DControl.BothMouseFunction = (Direct3DControl.MouseOption)comboBox3.SelectedItem;
            }
        }

		private void button1_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Shape newShape = ComplexShape.CreateFromFile(ofd.FileName);
				if (newShape != null)
				{
					direct3DControl.Engine.ShapeList.Add(newShape);
					direct3DControl.Engine.UpdateShapes();
					direct3DControl.SelectedObject = newShape;
				}
			}
		}

    }
}

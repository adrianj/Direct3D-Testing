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
            
            //if (direct3DControl.IsInitialized)
            //{
                InitTest();
            //}
        }

        private void InitTest()
		{
            direct3DControl.SelectedObjectChanged += (o, e) =>
            {
				object obj = direct3DControl.SelectedObject;
				if (obj is Shape)
				{
					Shape s = obj as Shape;
					propertyGrid.SelectedObject = s;
					textBox1.Text = "" + s;
				}
            };
			AddHerc();
			AddSphere();
			AddWoomera();
			direct3DControl.Engine.UpdateShapes();
             
        }

		private void AddHerc()
		{
			string filename = "C:\\Users\\adrianj\\Documents\\Work\\CAD\\hercules_LORES.stl";
			try
			{

				Shape shape = ComplexShapeFactory.CreateFromFile(filename);
				if (shape != null)
				{
					shape.Rotation = new Vector3(-(float)Math.PI / 2, 0, 0);
					direct3DControl.Engine.ShapeList.Add(shape);
				}
			}
			catch (System.IO.IOException) { MessageBox.Show("Create From File: " + filename + " failed"); }
		}

		private void AddSphere()
		{
			Sphere sp = new Sphere(24, Color.Black);
			sp.LongLines = 12;
			sp.LatLines = 7;
			sp.Scale = new Vector3(30, 30, 30);
			sp.Topology = SlimDX.Direct3D10.PrimitiveTopology.LineList;
			sp.CanPick = false;
			direct3DControl.Engine.ShapeList.Add(sp);
		}

		private void AddWoomera()
		{
			Shape square = new PictureTile();
			square.Scale = new Vector3(1000, 0, 1000);
			square.Location = new Vector3(0, -60, 0);
			square.TextureIndex = 3;
			direct3DControl.Engine.ShapeList.Add(square);
		}

        private void PreRenderTest()
        {

			textBox2.Text = "Refresh Rate: " + String.Format("{0:#00.00}",direct3DControl.Engine.RefreshRate)
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
			ofd.Filter = ComplexShapeFactory.SupportedFileTypeFilter;
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Shape newShape = ComplexShapeFactory.CreateFromFile(ofd.FileName);
				if (newShape != null)
				{
					direct3DControl.Engine.ShapeList.Add(newShape);
					direct3DControl.Engine.UpdateShapes();
					direct3DControl.SelectedObject = newShape;
				}
			}
		}

		private GoogleMapAccessor.GoogleTestForm googleMapForm;

		private void button2_Click(object sender, EventArgs e)
		{
			if (googleMapForm == null)
			{
				googleMapForm = new GoogleMapAccessor.GoogleTestForm();
				googleMapForm.FormClosing += (o, ev) => { ev.Cancel = true; googleMapForm.Hide(); };
			}
			googleMapForm.Show();
		}

    }
}

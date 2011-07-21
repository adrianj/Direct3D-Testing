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
			Shape shape = OpenShapeFromFile(filename);
			if (shape != null)
			{
				shape.Rotation = new Vector3(-(float)Math.PI / 2, 0, 0);
				shape.Location = new Vector3(0, 20.0f, 0);
				shape.Scale = new Vector3(0.01f, 0.01f, 0.01f);
				direct3DControl.Engine.ShapeList.Add(shape);
			}
		}

		private void AddSphere()
		{
			Sphere sp = new Sphere(24, Color.Black);
			sp.LongLines = 12;
			sp.LatLines = 7;
			sp.Scale = new Vector3(30, 30, 30);
			sp.Location = new Vector3(0, 20.0f, 0);
			sp.Topology = SlimDX.Direct3D10.PrimitiveTopology.LineList;
			sp.CanPick = false;
			direct3DControl.Engine.ShapeList.Add(sp);
		}

		private void AddWoomera()
		{
			string filename = "C:\\Users\\adrianj\\Documents\\Work\\CAD\\WebGIS_SRTM3\\Auckland.hgt";
			Shape shape = OpenShapeFromFile(filename);
			if (shape != null)
			{
				shape.Scale = new Vector3(0.01f, 0.1f, 0.01f);
				shape.Location = new Vector3(0, 0, 0);
				shape.TextureIndex = 3;
				direct3DControl.Engine.ShapeList.Add(shape);
			}
		}

		private Shape OpenShapeFromFile(string filename)
		{
			Shape shape = null;
			try
			{
				shape = ComplexShapeFactory.CreateFromFile(filename);
			}
			catch (System.IO.IOException)
			{
				MessageBox.Show("Create From File: " + filename + " failed");
			}
			return shape;
		}

        private void PreRenderTest()
        {

			textBox2.Text = "Refresh Rate: " + String.Format("{0:#00.00}",direct3DControl.Engine.RefreshRate)
				+ "\tLightDir: "+direct3DControl.Engine.LightDirection
				+ "\tCamTarget: "+direct3DControl.Target
				+ "\tCamTilt: " + direct3DControl.Tilt
				+ "\tCamPan: " + direct3DControl.Pan;
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

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
				if (obj is IRenderable)
				{
					IRenderable s = obj as IRenderable;
					propertyGrid.SelectedObject = s;
					textBox1.Text = "" + s;
				}
            };
			AddHerc();
			AddSphere();
			//AddWoomera();
			earth3DControl.UpdateShapes();
        }

		private void AddHerc()
		{
			string filename = "C:\\Users\\adrianj\\Documents\\Work\\CAD\\hercules_LORES.stl";
			Shape shape = OpenShapeFromFile(filename);
			if (shape != null)
			{
				shape.Rotation = new Vector3(-(float)Math.PI / 2, 0, 0);
				Float3 loc = new Float3(earth3DControl.CameraLocation);
				loc.Y = 500;
				shape.Location = loc.AsVector3();
				shape.Scale = new Vector3(1.0f, 1.0f, 1.0f);
				earth3DControl.ShapeList.Add(shape);
			}
		}

		private void AddSphere()
		{
			Sphere sp = new Sphere(24, Color.Black);
			sp.LongLines = 12;
			sp.LatLines = 7;
			sp.Scale = new Vector3(300, 300, 300);
			Float3 loc = new Float3(earth3DControl.CameraLocation);
			loc.Y = 500;
			sp.Location = loc.AsVector3();
			sp.Topology = SlimDX.Direct3D10.PrimitiveTopology.LineList;
			sp.CanPick = false;
			earth3DControl.ShapeList.Add(sp);
		}

		private void AddWoomera()
		{
			CombinedMapDataFactory.SetMapTerrainFolder("C:\\Users\\adrianj\\Documents\\Work\\CAD\\WebGIS_SRTM3");
			CombinedMapDataFactory.SetMapTextureFolder("C:\\Users\\adrianj\\Pictures\\Mapping\\GoogleTextures");
			Shape shape = ShapeHGTFactory.CreateFromCoordinates(-37.1, 174.1, 0.25, 0.25);
			if (shape != null)
			{
				shape.Scale = new Vector3(1.0f, 10.0f, 1.0f);
				shape.Location = new Vector3(0, 0, 0);
				shape.TextureIndex = 3;
				earth3DControl.ShapeList.Add(shape);
			}
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
			textBox2.Text = "Refresh Rate: " + String.Format("{0:#00.00}", earth3DControl.RefreshRate)
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

		private void button1_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = ComplexShapeFactory.SupportedFileTypeFilter;
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				Shape newShape = ComplexShapeFactory.CreateFromFile(ofd.FileName);
				if (newShape != null)
				{
					earth3DControl.ShapeList.Add(newShape);
					earth3DControl.UpdateShapes();
					earth3DControl.SelectedObject = newShape;
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
		}

    }
}

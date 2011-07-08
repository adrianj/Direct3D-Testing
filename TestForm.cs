using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SlimDX;

namespace Direct3DControl
{
    public partial class TestForm : Direct3DForm
    {
        public TestForm()
        {
            InitializeComponent();
            if (direct3DControl.IsInitialized)
            {
                InitTest();
            }
        }

        private void InitTest()
        {
            direct3DControl.CameraView.ZClipFar = 500;

            // What about an inside out cube...
            Shape shape = new Cube();
            shape.Scale = new Vector3(50, 50, 50);
            Array.Reverse(shape.Indices);
            //direct3DControl.AddShape(shape);
            // Build a room 10x10x10
            float rWidth = 10;
            shape = new Square(Color.LightBlue);
            shape.Location = new Vector3(0, 0, 0);
            shape.Scale = new Vector3(rWidth, rWidth, rWidth);
            direct3DControl.AddShape(shape);
            shape = new Square(Color.Orange);
            shape.Rotation = new Vector3(-0.5f * (float)Math.PI, 0, 0);
            shape.Location = new Vector3(1, 1, 1);
            shape.Scale = new Vector3(rWidth, rWidth, rWidth);
            //direct3DControl.AddShape(shape);
            shape = new Square(Color.YellowGreen);
            shape.Rotation = new Vector3(0, 0, 0.5f * (float)Math.PI);
            shape.Location = new Vector3(1, 1, 1);
            shape.Scale = new Vector3(rWidth, rWidth, rWidth);
            //direct3DControl.AddShape(shape);


            // Populate with some cubes
            shape = new Cube();
            //shape.Location = new Vector3(1, 1, 1);
            //shape.Location += new Vector3(4, 0, 2);
            //shape.Rotation = new Vector3(0, (float)Math.PI / 4, 0);
            shape.Name = "smallCube";
            direct3DControl.AddShape(shape);

            shape = new Cube();
            shape.Location = new Vector3(1, 1, 1);
            shape.Location += new Vector3(8, 0, 5);
            shape.Scale = new Vector3(3, 3, 3);
            shape.Rotation = new Vector3(0, (float)Math.PI / 2, 0);
            shape.Name = "largeCube";
            direct3DControl.AddShape(shape);

            // Now for a really small Cube... aka, a line.
            // This becomes the X-Axis.
            float lineWidth = 0.02f;
            shape = new Cube(Color.Black);
            shape.Scale = new Vector3(1000, lineWidth, lineWidth);
            direct3DControl.AddShape(shape);
            // And the Y-Axis
            shape = new Cube(Color.Black);
            shape.Scale = new Vector3(lineWidth, 1000, lineWidth);
            direct3DControl.AddShape(shape);
            // And the Z-Axis
            shape = new Cube(Color.Black);
            shape.Scale = new Vector3(lineWidth, lineWidth, 1000);
            direct3DControl.AddShape(shape);

            shape = new Triangle();
            direct3DControl.MouseClick += new MouseEventHandler(direct3DControl_MouseClick);
        }

        void direct3DControl_MouseClick(object sender, MouseEventArgs e)
        {
            // Attempt to select an object at the mouse location.
            Shape obj = direct3DControl.PickObjectAt(e.Location);
            textBox2.Text = "" + obj;
            if (obj != null)
                propertyGrid.SelectedObject = obj;
        }


        private void PreRenderTest()
        {
            textBox1.Text = "CamLoc:   " + direct3DControl.CameraView.Location  +
                ",   Pan: " + direct3DControl.CameraView.Pan + ", Tilt:   " + direct3DControl.CameraView.Tilt
                 + ", Zoom:   " + direct3DControl.CameraView.Zoom;
        }

        public override void Render()
        {
            PreRenderTest();
            if(direct3DControl.IsInitialized)
                direct3DControl.Render();
        }

    }
}

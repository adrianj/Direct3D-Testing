using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SlimDX;

namespace Direct3DControl
{
    public partial class Direct3DControl : UserControl
    {
        private Direct3DEngine engine;
        public bool IsInitialized
        {
            get { if (engine == null) return false; return engine.IsInitialized; }
        }
        public Direct3DControl()
        {
            InitializeComponent();
            InitializeDevice();
            InitializeMouse();
            InitializeKeyboard();
        }

        private void InitializeDevice()
        {
            engine = new Direct3DEngine(this);
            
        }

        public float CameraPan { get { return engine.CameraView.Pan; } set { engine.CameraView.Pan = value; } }
        public float CameraTilt { get { return engine.CameraView.Tilt; } set { engine.CameraView.Tilt = value; } }
        public float CameraZoom { get { return engine.CameraView.Zoom; } set { engine.CameraView.Zoom = value; } }
        public Vector3 CameraLocation { get { return engine.CameraView.Location; } set { engine.CameraView.Location = value; } }

        public CameraControl CameraView { get { return engine.CameraView; } set { engine.CameraView = value; } }

        public void AddShape(Shape s)
        {
            engine.AddShape(s);
        }

        public void Render()
        {
            if (engine != null) engine.Render();
        }

        public Shape PickObjectAt(Point screenLocation)
        { return engine.PickObjectAt(screenLocation); }

        #region Mouse Clicks
        private Point mouseDownPoint;
        private void InitializeMouse()
        {
            this.MouseMove += new MouseEventHandler(mParent_MouseMove);
            this.MouseDoubleClick += new MouseEventHandler(mParent_MouseDoubleClick);
            this.Leave += new EventHandler(Direct3DControl_Leave);
        }

        void Direct3DControl_Leave(object sender, EventArgs e)
        {
            keyDownList.Clear();
            keyMoveTimer.Stop();
        }


        void mParent_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            engine.CameraView.Initialize();
        }

        void mParent_MouseMove(object sender, MouseEventArgs e)
        {
            float xDiff = (float)(e.X - mouseDownPoint.X) / 20;
            float yDiff = (float)(e.Y - mouseDownPoint.Y) / 20;
            if (e.Button == MouseButtons.Left)
            {
                engine.CameraView.Pan -= xDiff / 5;
                engine.CameraView.Tilt -= yDiff;
            }
            else if (e.Button == MouseButtons.Right)
            {
                engine.CameraView.Translate(xDiff, 0, yDiff);
            }
            else if (e.Button == (MouseButtons.Left | MouseButtons.Right))
            {
                engine.CameraView.Zoom += yDiff * engine.CameraView.Zoom;
            }

            mouseDownPoint = e.Location;
        }
        #endregion


        #region Key Presses

        private List<Keys> keyDownList = new List<Keys>();
        private List<Keys> keysOfInterest = new List<Keys>() { Keys.W, Keys.A, Keys.S, Keys.D, Keys.Q,Keys.E };
        private Timer keyMoveTimer = new Timer();
        private bool keyShift = false;

        private void InitializeKeyboard()
        {
            this.KeyDown += new KeyEventHandler(Direct3DControl_KeyDown);
            this.KeyUp += new KeyEventHandler(Direct3DControl_KeyUp);
            keyMoveTimer.Interval = 10;
            keyMoveTimer.Tick += new EventHandler(keyMoveTimer_Tick);
        }

        void keyMoveTimer_Tick(object sender, EventArgs e)
        {
            float keyWS = 0;
            float keyAD = 0;
            float keyQE = 0;
            float f = 0.05f;
            if (keyDownList.Contains(Keys.W))
                keyWS += f;
            if (keyDownList.Contains(Keys.S))
                keyWS -= f;
            if (keyDownList.Contains(Keys.A))
                keyAD -= f;
            if (keyDownList.Contains(Keys.D))
                keyAD += f;
            if (keyDownList.Contains(Keys.Q))
                keyQE -= f;
            if (keyDownList.Contains(Keys.E))
                keyQE += f;
            if (keyShift)
            {
                engine.CameraView.Tilt += keyWS;
                engine.CameraView.Pan -= keyAD;
            }
            else
            {
                engine.CameraView.Translate(-keyAD, keyQE, keyWS);
            }
        }

        void Direct3DControl_KeyUp(object sender, KeyEventArgs e)
        {
            keyDownList.Remove(e.KeyCode);
            if (keyDownList.Count < 1)
                keyMoveTimer.Stop();
        }

        void Direct3DControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (!keysOfInterest.Contains(e.KeyCode)) return;
            if (!keyDownList.Contains(e.KeyCode)) keyDownList.Add(e.KeyCode);
            keyShift = e.Shift;
            keyMoveTimer.Start();
        }
        #endregion
    }
}

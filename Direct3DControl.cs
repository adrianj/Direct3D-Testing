using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Windows;
using System.Reflection;

namespace Direct3DLib
{
    public partial class Direct3DControl : UserControl
    {
        private Direct3DEngine engine;
		public Direct3DEngine Engine { get { return engine; } }
		public bool IsInitialized { get { if (engine == null) return false; return engine.IsInitialized; } }
        
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

		#region Public Properties
		[CategoryAttribute("Camera and Lighting")]
		public float Tilt { get { return engine.Camera.Tilt; } set { engine.Camera.Tilt = value; } }
		[CategoryAttribute("Camera and Lighting")]
		public float Pan { get { return engine.Camera.Pan; } set { engine.Camera.Pan = value; } }
		[CategoryAttribute("Camera and Lighting")]
		public Vector3 Target { get { return engine.Camera.Location; } set { engine.Camera.Location = value; } }
		[CategoryAttribute("Camera and Lighting")]
		public float Zoom { get { return engine.Camera.Zoom; } set { engine.Camera.Zoom = value; } }
		[CategoryAttribute("Camera and Lighting")]
		public float ZClipNear { get { return engine.Camera.ZClipNear; } set { engine.Camera.ZClipNear = value; } }
		[CategoryAttribute("Camera and Lighting")]
		public float ZClipFar { get { return engine.Camera.ZClipFar; } set { engine.Camera.ZClipFar = value; } }
		[CategoryAttribute("Camera and Lighting")]
		public Vector3 LightDirection { get { return engine.LightDirection; } set { engine.LightDirection = value; } }
		[CategoryAttribute("Camera and Lighting")]
		public float LightDirectionalIntensity { get { return engine.LightDirectionalIntensity; } set { engine.LightDirectionalIntensity = value; } }
		[CategoryAttribute("Camera and Lighting")]
		public float LightAmbientIntensity { get { return engine.LightAmbientIntensity; } set { engine.LightAmbientIntensity = value; } }
		#endregion

		#region Events
		private object mSelectedObject = null;
        public object SelectedObject { get { return mSelectedObject; } set { mSelectedObject = value; FireSelectedObjectChangedEvent(value); } }
        public event PropertyChangedEventHandler SelectedObjectChanged;
        private void FireSelectedObjectChangedEvent(object selectedObj)
        {
            if (SelectedObjectChanged != null) SelectedObjectChanged(this, new PropertyChangedEventArgs("SelectedObject"));
        }

        public event EventHandler CameraChanged
        {
			add { Engine.Camera.CameraChanged += value; }
			remove { Engine.Camera.CameraChanged -= value; }
        }
        #endregion


        public void Render()
        {
			// Only render if the Engine is initialized and this application is the active form.
			if (IsInitialized && Form.ActiveForm != null)
			{
				engine.Render();
			}
		
        }

        public IRenderable PickObjectAt(Point screenLocation)
        { return engine.PickObjectAt(screenLocation); }

        #region Mouse Clicks
        public enum MouseOption { None, Select, Rotate, CameraTranslateXZ, Zoom };
        private MouseOption mLeftMouse = MouseOption.Select;
        /// <summary>
        /// Function to perform when the left mouse button is clicked.
        /// </summary>
        [CategoryAttribute("Mouse Functions")]
        public MouseOption LeftMouseFunction { get { return mLeftMouse; } set { mLeftMouse = value; } }
        private MouseOption mRightMouse = MouseOption.Rotate;
        /// <summary>
        /// Function to pertorm when the right mouse button is clicked.
        /// </summary>
        [CategoryAttribute("Mouse Functions")]
        public MouseOption RightMouseFunction { get { return mRightMouse; } set { mRightMouse = value; } }
        private MouseOption mBothMouse = MouseOption.CameraTranslateXZ;
        /// <summary>
        /// Function to perform when both mouse buttons are held down. NOTE: Select function does not work correctly
        /// for both mouse buttons.
        /// </summary>
        [CategoryAttribute("Mouse Functions")]
        public MouseOption BothMouseFunction { get { return mBothMouse; } set { mBothMouse = value; } }
        private Point mouseDownPoint;
        private void InitializeMouse()
        {
            this.MouseMove += new MouseEventHandler(this_MouseMove);
            //this.MouseDoubleClick += new MouseEventHandler(this_MouseDoubleClick);
            this.MouseUp += new MouseEventHandler(this_MouseUp);
            this.Leave += new EventHandler(Direct3DControl_Leave);
        }

        void Direct3DControl_Leave(object sender, EventArgs e)
        {
            keyDownList.Clear();
            keyMoveTimer.Stop();
        }

        void this_MouseUp(object sender, MouseEventArgs e)
        {
            // Attempt to select an object at the mouse location.

            if ((e.Button == MouseButtons.Left && LeftMouseFunction == MouseOption.Select)
                || (e.Button == MouseButtons.Right && RightMouseFunction == MouseOption.Select))
            {
                IRenderable obj = this.PickObjectAt(e.Location);
                if (obj != null)
                    SelectedObject = obj;
            }
        }

        void this_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            engine.Camera.Initialize();
        }

        void this_MouseMove(object sender, MouseEventArgs e)
        {
            float xDiff = (float)(e.X - mouseDownPoint.X);
            float yDiff = (float)(e.Y - mouseDownPoint.Y);
            if (e.Button != System.Windows.Forms.MouseButtons.None)
            {
                if ((e.Button == MouseButtons.Left && LeftMouseFunction == MouseOption.Rotate)
                    || (e.Button == MouseButtons.Right && RightMouseFunction == MouseOption.Rotate)
                    || (e.Button == (MouseButtons.Left | MouseButtons.Right) && BothMouseFunction == MouseOption.Rotate))
                {
                    engine.Camera.Pan -= xDiff / 80;
                    engine.Camera.Tilt -= yDiff / 20;
                }
                else if ((e.Button == MouseButtons.Left && LeftMouseFunction == MouseOption.CameraTranslateXZ)
                    || (e.Button == MouseButtons.Right && RightMouseFunction == MouseOption.CameraTranslateXZ)
                    || (e.Button == (MouseButtons.Left | MouseButtons.Right) && BothMouseFunction == MouseOption.CameraTranslateXZ))
                {
                    engine.Camera.Translate(xDiff/50*Zoom, 0, yDiff/50*Zoom);
                }
                else if ((e.Button == MouseButtons.Left && LeftMouseFunction == MouseOption.Zoom)
                    || (e.Button == MouseButtons.Right && RightMouseFunction == MouseOption.Zoom)
                    || (e.Button == (MouseButtons.Left | MouseButtons.Right) && BothMouseFunction == MouseOption.Zoom))
                {
                    engine.Camera.Zoom += yDiff * (float)Math.Ceiling(engine.Camera.Zoom)/50;
                }
            }
            mouseDownPoint = e.Location;
        }
        #endregion


        #region Key Presses

        private List<Keys> keyDownList = new List<Keys>();
        private List<Keys> keysOfInterest = new List<Keys>() { Keys.W, Keys.A, Keys.S, Keys.D, Keys.Q,Keys.E, Keys.Delete };
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
			float f = 0.02f;
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
                engine.Camera.Tilt += keyWS;
                engine.Camera.Pan -= keyAD;
            }
            else
            {
                engine.Camera.Translate(-keyAD*Zoom, keyQE*Zoom, keyWS*Zoom);
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
			if (e.KeyCode == Keys.Delete)
			{
				if (SelectedObject != null && SelectedObject is IRenderable)
				{
					IRenderable shape = SelectedObject as IRenderable;
					if (engine.ShapeList.Contains(shape))
					{
						engine.ShapeList.Remove(shape);
						SelectedObject = null;
					}
				}
			}
			else
			{
				if (!keyDownList.Contains(e.KeyCode)) keyDownList.Add(e.KeyCode);
				keyShift = e.Shift;
				keyMoveTimer.Start();
			}
        }
        #endregion


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using SlimDX;
using SlimDX.Windows;
using System.Reflection;

namespace Direct3DLib
{
	[Designer(typeof(Direct3DDesigner),typeof(IDesigner))]
	public partial class Direct3DControl : UserControl
	{
		private Direct3DEngine engine;
		protected Direct3DEngine Engine { get { return engine; } }
		public bool IsInitialized { get { if (engine == null) return false; return engine.IsInitialized; } }
		private bool designTime = false;
		private bool forceRender = true;

		public Direct3DControl()
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				designTime = true;
			engine = new Direct3DEngine(this);
			InitializeComponent();
			InitializeMouse();
			InitializeKeyboard();
			this.Load += new EventHandler(Direct3DControl_Load);
		}

		void Direct3DControl_Load(object sender, EventArgs e)
		{
			//if (!designTime)
			//{
				InitializeDevice();
				InitializeShapes();
			//}
		}

		private void InitializeDevice()
		{
			try
			{
				engine.InitializeDevice();
				if (!engine.IsInitialized) throw new Exception("Unknown Error");
			}
			catch (Exception ex)
			{

				MessageBox.Show("Direct3D Engine Initialization Failed\n\n" + ex);
			}
		}

		private void InitializeShapes()
		{
			List<Shape> initialShapes = Shape.GetInitialShapes();
			foreach (Shape shape in initialShapes)
			{
				if (!shape.IsDisposed && !engine.ShapeList.Contains(shape))
					engine.ShapeList.Add(shape);
			}
		}

		#region Public Properties
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float CameraTilt { get { return engine.Camera.Tilt; } set { engine.Camera.Tilt = value; if (designTime) this.Invalidate(); } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float CameraPan { get { return engine.Camera.Pan; } set { engine.Camera.Pan = value; if (designTime) this.Invalidate(); } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public Float3 CameraLocation { get { return new Float3(engine.Camera.Location); } set { engine.Camera.Location = value.AsVector3(); if (designTime) this.Invalidate(); } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float Zoom { get { return engine.Camera.Zoom; } set { engine.Camera.Zoom = value; if (designTime) this.Invalidate(); } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float ZClipNear { get { return engine.Camera.ZClipNear; } set { engine.Camera.ZClipNear = value; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float ZClipFar { get { return engine.Camera.ZClipFar; } set { engine.Camera.ZClipFar = value; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		[Browsable(false)]
		public Matrix CameraView { get { return engine.Camera.View; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		[Browsable(false)]
		public Matrix CameraProjection { get { return engine.Camera.Proj; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public Float3 LightDirection { get { return new Float3(engine.LightDirection); } set { engine.LightDirection = value.AsVector3(); } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float LightDirectionalIntensity { get { return engine.LightDirectionalIntensity; } set { engine.LightDirectionalIntensity = value; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float LightAmbientIntensity { get { return engine.LightAmbientIntensity; } set { engine.LightAmbientIntensity = value; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[EditorAttribute(typeof(NoEditor),typeof(UITypeEditor))]
		public Image[] TextureImages { get { return engine.TextureImages; } set { engine.TextureImages = value; } }

		private object mSelectedObject = null;
		[TypeConverter(typeof(BasicTypeConverter))]
		public object SelectedObject { get { return mSelectedObject; } set { mSelectedObject = value; FireSelectedObjectChangedEvent(value); } }
		[Browsable(false)]
		public double RefreshRate { get { return engine.RefreshRate; } }
		#endregion

		#region Events
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


		/// <summary>
		/// Forces the control to render the scene. Otherwise, the scene is only rendered when the top level form
		/// is the currently active form.
		/// </summary>
		public void ForceRender()
		{
			forceRender = true;
		}

		public void Render()
		{
			// Only render if the Engine is initialized and this application is the active form, or user forces it to.
			if (IsInitialized && (Form.ActiveForm != null || forceRender || this.designTime) && this.Visible)
			{
				engine.Render();
				forceRender = false;
			}

		}
		public void UpdateShapes()
		{
			engine.UpdateAllShapes();
		}


		public Shape PickObjectAt(Point screenLocation)
		{
			return engine.PickObjectAt(screenLocation);
		}

		#region Mouse Clicks
		public enum MouseOption { None, Select, Rotate, CameraTranslateXZ, Zoom };
		private MouseOption mLeftMouse = MouseOption.Select;
		/// <summary>
		/// Function to perform when the left mouse button is clicked.
		/// </summary>
		[CategoryAttribute("Mouse and Keyboard Functions")]
		public MouseOption LeftMouseFunction { get { return mLeftMouse; } set { mLeftMouse = value; } }
		private MouseOption mRightMouse = MouseOption.Rotate;
		/// <summary>
		/// Function to pertorm when the right mouse button is clicked.
		/// </summary>
		[CategoryAttribute("Mouse and Keyboard Functions")]
		public MouseOption RightMouseFunction { get { return mRightMouse; } set { mRightMouse = value; } }
		private MouseOption mBothMouse = MouseOption.CameraTranslateXZ;
		/// <summary>
		/// Function to perform when both mouse buttons are held down. NOTE: Select function does not work correctly
		/// for both mouse buttons.
		/// </summary>
		[CategoryAttribute("Mouse and Keyboard Functions")]
		public MouseOption BothMouseFunction { get { return mBothMouse; } set { mBothMouse = value; } }
		private Point mouseDownPoint;

		private float mouseSpeed = 50;
		[CategoryAttribute("Mouse and Keyboard Functions")]
		public float MouseMovementSpeed { get { return mouseSpeed; } set { mouseSpeed = value; } }

		private void InitializeMouse()
		{
			this.MouseMove += new MouseEventHandler(this_MouseMove);
			this.MouseUp += new MouseEventHandler(this_MouseUp);
			this.Leave += new EventHandler(Direct3DControl_Leave);
		}

		public void RotateCamera(float pan, float tilt)
		{
			CameraPan += pan;
			CameraTilt += tilt;
		}

		public void TranslateCamera(float x, float y, float z)
		{
			engine.Camera.Translate(x, y, z);
			if (this.designTime) Invalidate();
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
				Shape obj = this.PickObjectAt(e.Location);
				if (obj != null)
					SelectedObject = obj;
			}
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
					float panChange = xDiff / 100f;
					float tiltChange = yDiff / 30f;
					RotateCamera(-panChange,-tiltChange);
				}
				else if ((e.Button == MouseButtons.Left && LeftMouseFunction == MouseOption.CameraTranslateXZ)
					|| (e.Button == MouseButtons.Right && RightMouseFunction == MouseOption.CameraTranslateXZ)
					|| (e.Button == (MouseButtons.Left | MouseButtons.Right) && BothMouseFunction == MouseOption.CameraTranslateXZ))
				{
					TranslateCamera(xDiff / 50 * mouseSpeed, 0, yDiff / 50 * mouseSpeed);
				}
				else if ((e.Button == MouseButtons.Left && LeftMouseFunction == MouseOption.Zoom)
					|| (e.Button == MouseButtons.Right && RightMouseFunction == MouseOption.Zoom)
					|| (e.Button == (MouseButtons.Left | MouseButtons.Right) && BothMouseFunction == MouseOption.Zoom))
				{
					engine.Camera.Zoom += yDiff * (float)Math.Ceiling(engine.Camera.Zoom) / 50;
				}
			}
			mouseDownPoint = e.Location;
		}
		#endregion

		#region Design Time Mouse Clicks
		public void ProcessDesignTimeMouseMessage(ref Message m)
		{
			if (m.Msg >= WndProcMessage.MIN_MOUSE && m.Msg <= WndProcMessage.MAX_MOUSE)
			{
				if (m.Msg == WndProcMessage.WM_MOUSEMOVE)
				{
					OnMouseMove(BuildArgs(m));
				}
				if (m.Msg == WndProcMessage.WM_LBUTTONDOWN)
				{
					OnMouseDown(BuildArgs(m));
				}
				if (m.Msg == WndProcMessage.WM_LBUTTONUP)
				{
					OnMouseUp(BuildArgs(m,MouseButtons.Left));
				}
				if (m.Msg == WndProcMessage.WM_RBUTTONDOWN)
				{
					OnMouseDown(BuildArgs(m));
				}
				if (m.Msg == WndProcMessage.WM_RBUTTONUP)
				{
					OnMouseUp(BuildArgs(m, MouseButtons.Left));
				}
			}
		}
		private MouseEventArgs BuildArgs(Message m)
		{
			return BuildArgs(m, MouseButtons.None);
		}
		private MouseEventArgs BuildArgs(Message m, MouseButtons sourceButton)
		{
			MouseButtons mb = sourceButton;
			Point p = new Point(m.LParam.ToInt32());
			int wParam = m.WParam.ToInt32();
			if ((wParam & 0x0001) == 0x0001)
				mb |= MouseButtons.Left;
			if ((wParam & 0x0002) == 0x0002)
				mb |= MouseButtons.Right;
			if ((wParam & 0x0010) == 0x0010)
				mb |= MouseButtons.Middle;
			if ((wParam & 0x0020) == 0x0020)
				mb |= MouseButtons.XButton1;
			if ((wParam & 0x0040) == 0x0040)
				mb |= MouseButtons.XButton2;
			
			MouseEventArgs e = new MouseEventArgs(mb, 1, p.X, p.Y, 0);
			
			return e;
		}
		#endregion
		#region Key Presses

		private List<Keys> keyDownList = new List<Keys>();
		private List<Keys> keysOfInterest = new List<Keys>() { Keys.W, Keys.A, Keys.S, Keys.D, Keys.Q, Keys.E, Keys.Delete };
		private Timer keyMoveTimer = new Timer();
		private bool keyShift = false;

		private float keySpeed = 50;
		[CategoryAttribute("Mouse and Keyboard Functions")]
		public float KeyboardMovementSpeed { get { return keySpeed; } set { keySpeed = value; } }

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
				RotateCamera(-keyAD, keyWS);
			}
			else
			{
				TranslateCamera(-keyAD * keySpeed, keyQE * keySpeed, keyWS * keySpeed);
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
				if (SelectedObject != null && SelectedObject is Shape)
				{
					Shape shape = SelectedObject as Shape;
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

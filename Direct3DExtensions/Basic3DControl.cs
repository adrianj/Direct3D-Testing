using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;

namespace Direct3DExtensions
{
	public interface Direct3DEngine
	{
		bool IsDesignMode { get; }
		CameraInput CameraInput { get; set; }
		D3DDevice D3DDevice { get; }
		Effect Effect { get; }
		Geometry Geometry { get; }
		void InitializeDirect3D();
	}

	public class Basic3DControl : UserControl, Direct3DEngine
	{
		public bool IsDesignMode { get; private set; }
		public CameraInput CameraInput { get; set; }
		public D3DDevice D3DDevice { get; protected set; }
		public Effect Effect { get; protected set; }
		public Geometry Geometry { get; protected set; }

		private bool initSuccessful = false;

		public Basic3DControl()
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				IsDesignMode = true;
			this.BackColor = System.Drawing.Color.LightBlue;
		}

		public virtual void InitializeDirect3D()
		{
			if (initSuccessful) return;
			try
			{
				
				Console.WriteLine("init");
				InitCameraInput();
				InitDevice();
				InitEffect();
				InitGeometry();
				Application.Idle += Application_Idle;
				initSuccessful = true;
				Console.WriteLine("init done");
			}
			catch (Exception ex)
			{
				throw new SlimDXException("Direct3D failed to initialize.", ex);
			}
		}

		protected virtual void InitCameraInput()
		{
			if (CameraInput == null)
			{
				InputHelper input = new InputHelper(this);
				Camera camera = new Camera();
				camera.LookAt(new Vector3(0.5f, 1.5f, 0), new Vector3(0.5f, 0, 0.5f));
				camera.Persepective(45.0f * (float)Math.PI / 180.0f, ClientSize.Width / (float)ClientSize.Height, 0.025f, 1200.0f);

				CameraInput = new FirstPersonCameraInput(camera, input);
			}
		}

		protected virtual void InitDevice()
		{
			if (D3DDevice != null) D3DDevice.Dispose();
			D3DDevice = new D3DDevice();
			D3DDevice.Init(this);
			this.SizeChanged += (o, e) => { this.UpdateSize(); };
		}

		protected virtual void InitEffect()
		{
			if (Effect != null) Effect.Dispose();
			Effect = new BasicEffect();
			Effect.Init(D3DDevice);
		}

		protected virtual void InitGeometry()
		{
			if (Geometry != null) Geometry.Dispose();
			Geometry = new BasicGeometry();

			Mesh mesh = CreateSimpleMesh();
			Geometry.Meshes.Add(mesh);

			Geometry.Init(D3DDevice, Effect, 0);
		}

		private Mesh CreateSimpleMesh()
		{
			Mesh mesh = new BasicMesh();
			mesh.Vertices = new Vector3[12];
			mesh.Vertices[0] = new Vector3(0, 0, 0);
			mesh.Vertices[1] = new Vector3(0, 0, 1);
			mesh.Vertices[2] = new Vector3(1, 0, 1);
			mesh.Vertices[3] = new Vector3(1, 0, 0);
			mesh.Vertices[4] = new Vector3(0, 0, 2);
			mesh.Vertices[5] = new Vector3(1, 0, 2);
			mesh.Vertices[6] = new Vector3(2, 0, 1);
			mesh.Vertices[7] = new Vector3(2, 0, 0);
			mesh.Vertices[8] = new Vector3(1, 0, -1);
			mesh.Vertices[9] = new Vector3(0, 0, -1);
			mesh.Vertices[10] = new Vector3(-1, 0, 0);
			mesh.Vertices[11] = new Vector3(-1, 0, 1);
			mesh.FaceVertices = new uint[] { 
				0,1,2,
				0,2,3,
				1,4,2,
				4,5,2,
				2,6,7,
				2,7,3,
				3,8,0,
				0,8,9,
				0,10,11,
				0,11,1
			};
			mesh.FaceCount = mesh.FaceVertices.Length / 3;

			return mesh;
		}

		void Application_Idle(object sender, System.EventArgs e)
		{
			while (SlimDX.Windows.MessagePump.IsApplicationIdle)
			{
				if(this.Visible && this.ParentForm.WindowState != FormWindowState.Minimized)
					Render();
			}
		}

		protected virtual void UpdateSize()
		{
			if (this.Width < 1 || this.Height < 1) return;
			CameraInput.SetSize(this.Width, this.Height);
			D3DDevice.SetSize(this.Width, this.Height);
		}

		protected virtual void Render()
		{
			CameraInput.OnRender();
			D3DDevice.Clear();
			Effect.ApplyAll(CameraInput.Camera);
			Geometry.Draw();
			D3DDevice.Present();
		}

		protected override void Dispose(bool disposing)
		{
			Application.Idle -= Application_Idle;
			if(D3DDevice != null) D3DDevice.Dispose();
			if (Effect != null) Effect.Dispose();
			if (Geometry != null) Geometry.Dispose();
			initSuccessful = false;
			base.Dispose(disposing);
		}
	}
}

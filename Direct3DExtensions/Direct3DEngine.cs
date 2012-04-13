using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;

namespace Direct3DExtensions
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface Direct3DEngine : IDisposable, IComponent
	{
		event EventHandler InitializationComplete;
		event RenderEventHandler PostRendering;
		event RenderEventHandler PreRendering;
		event CameraChangedEventHandler CameraChanged;
		CameraInput CameraInput { get; set; }
		D3DDevice D3DDevice { get; }
		Effect Effect { get; set; }
		Geometry Geometry { get; set; }
		D3DHostControl HostControl { get; set; }
		bool InitSuccessful { get; }
		void InitializeDirect3D();
		void BindMesh(Mesh mesh, int passIndex);
		void BindMesh(Mesh mesh, string passName);
	}
	public delegate void RenderEventHandler(object sender, RenderEventArgs e);
	public class RenderEventArgs : EventArgs
	{
		public double TimeDelta { get; private set; }
		public RenderEventArgs(double delta) { TimeDelta = delta; }
	}

	public class Basic3DEngine : Component, Direct3DEngine
	{
		public CameraInput CameraInput { get; set; }
		public D3DDevice D3DDevice { get; set; }
		public virtual Effect Effect { get; set; }
		public virtual Geometry Geometry { get; set; }


		public bool InitSuccessful { get; private set; }
		D3DHostControl hostControl;
		public D3DHostControl HostControl { get { return hostControl; } set { SetParent(value); } }

		public event EventHandler InitializationComplete;
		public event RenderEventHandler PostRendering;
		public event RenderEventHandler PreRendering;
		public event CameraChangedEventHandler CameraChanged;

		protected void FirePostRendering()
		{
			if (PostRendering == null || !InitSuccessful) return;
			RenderEventArgs e = new RenderEventArgs(CameraInput.TimeDelta);
			PostRendering(this, e);
		}

		protected void FirePreRendering()
		{
			if (PreRendering == null || !InitSuccessful) return;
			RenderEventArgs e = new RenderEventArgs(CameraInput.TimeDelta);
			PreRendering(this, e);
		}

		protected void FireInitializationComplete()
		{
			if (InitializationComplete != null)
				InitializationComplete(this, EventArgs.Empty);
		}

		protected void FireCameraChanged(CameraChangedEventArgs e)
		{
			if (CameraChanged != null && InitSuccessful)
				CameraChanged(this, e);
		}

		void SetParent(D3DHostControl parent)
		{
			this.hostControl = parent;
			this.hostControl.SetEngine(this);
		}


		public virtual void InitializeDirect3D()
		{
			if (InitSuccessful) return;
				InitDevice();
				InitEffect();
				InitGeometry();
				InitCameraInput();
				Application.Idle += Application_Idle;
				FireInitializationComplete();
				InitSuccessful = true;
		}

		protected virtual void InitCameraInput()
		{
			if (CameraInput == null)
			{
				CameraInput = new FirstPersonCameraInput(HostControl);
				CameraInput.Camera.UpdatePerspective(90.0f * (float)Math.PI / 180.0f, HostControl.ClientSize.Width / (float)HostControl.ClientSize.Height, 0.025f, 1200.0f);
				CameraInput.LookAt(new Vector3(-1.5f, 1.5f, -1.5f), new Vector3(0.5f, 0, 0.5f));
			}
			CameraInput.CameraChanged += (o, e) => { this.FireCameraChanged(e); };
		}

		protected virtual void InitDevice()
		{
			if (D3DDevice == null)
				D3DDevice = new D3DDevice();
			D3DDevice.Init(HostControl);
			HostControl.SizeChanged += (o, e) => { this.UpdateSize(); };
		}

		protected virtual void InitEffect()
		{

			if (Effect == null)
				Effect = new WorldViewProjEffect();
			Effect.Init(D3DDevice);
		}

		protected virtual void InitGeometry()
		{
			if (Geometry == null) 
				Geometry = new BasicGeometry();
		}

		void Application_Idle(object sender, System.EventArgs e)
		{
			while (SlimDX.Windows.MessagePump.IsApplicationIdle)
			{
				if (!HostControl.Visible
					|| HostControl.ParentForm.WindowState == FormWindowState.Minimized
					|| !HostControl.Focused)
				{
					System.Threading.Thread.Yield();
					continue;
				}
				Render();
			}
		}

		protected virtual void UpdateSize()
		{
			if (HostControl.Width < 1 || HostControl.Height < 1) return;
			CameraInput.SetSize(HostControl.Width, HostControl.Height);
			
			D3DDevice.SetSize(HostControl.Width, HostControl.Height);
		}

		protected virtual void Render()
		{
			CameraInput.OnRender();
			D3DDevice.Clear(); 
			FirePreRendering();
			ApplyCameraToEffect();
			DrawGeometry();
			D3DDevice.Present();
			FirePostRendering();
		}

		protected virtual void ApplyCameraToEffect()
		{
			Effect.SetCamera(CameraInput.Camera);
		}

		protected virtual void DrawGeometry()
		{
			Geometry.Draw();
		}

		public virtual void BindMesh(Mesh mesh, string passName)
		{
			if (Effect == null || D3DDevice == null)
				throw new Exception("Cannot bind mesh before initialization successful.");
			int passIndex = Effect.GetPassIndexByName(passName);
			if (passIndex < 0) passIndex = 0;
			BindMesh(mesh, passIndex);
		}

		public void BindMesh(Mesh mesh, int passIndex)
		{
			if (Effect == null || D3DDevice == null)
				throw new Exception("Cannot bind mesh before initialization successful.");
			mesh.BindToPass(D3DDevice, Effect, passIndex);
			Geometry.Add(mesh);
		}


		void DisposeManaged() { }
		void DisposeUnmanaged()
		{
			Application.Idle -= Application_Idle;
			if (Effect != null) Effect.Dispose();
			Effect = null;
			if (Geometry != null) Geometry.Dispose();
			Geometry = null;
			if (D3DDevice != null) D3DDevice.Dispose();
			D3DDevice = null;
			InitSuccessful = false;
		}

		bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}

				DisposeUnmanaged();
				disposed = true;
			}
			base.Dispose(disposing);
		}
    


		public static Mesh CreateSimpleMesh()
		{
			Mesh mesh = new BasicMesh();
			Vector3[] pos = new Vector3[] {
			new Vector3(0, 0, 0),
			new Vector3(0, 0, 1),
			new Vector3(1, 0, 1),
			new Vector3(1, 0, 0),
			new Vector3(0, 0, 2),
			new Vector3(1, 0, 2),
			new Vector3(2, 0, 1),
			new Vector3(2, 0, 0),
			new Vector3(1, 0, -1),
			new Vector3(0, 0, -1),
			new Vector3(-1, 0, 0),
			new Vector3(-1, 0, 1)};
			mesh.Vertices = new Vertex[pos.Length];
			for (int i = 0; i < pos.Length; i++)
				mesh.Vertices[i] = new VertexTypes.PositionTexture() { Pos = pos[i], TexCoord = new Vector2(pos[i].X, pos[i].Z) };
			mesh.Indices = new int[] { 
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


			return mesh;
		}
	}

	
}

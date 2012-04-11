/* 
MIT License

Copyright (c) 2009 Brad Blanchard (www.linedef.com)

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

namespace Direct3DExtensions
{
	using System;
	using SlimDX;

	using System.ComponentModel;

	public class CameraChangedEventArgs : EventArgs
	{
		public CameraChangedEventArgs(bool positionChanged, bool viewChanged, bool projectionChanged, Camera camera)
		{
			this.PositionChanged = positionChanged;
			this.ProjectionChanged = projectionChanged;
			this.ViewChanged = viewChanged;
			this.Camera = camera;
		}

		public bool PositionChanged { get; private set; }
		public bool ViewChanged { get; private set; }
		public bool ProjectionChanged { get; private set; }
		public Camera Camera { get; private set; }
	}

	public delegate void CameraChangedEventHandler(object sender, CameraChangedEventArgs e);

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class Camera
	{
		bool freezeUpdates = false;
		float fov, aspect, nearZ, farZ;
		Vector3 position, target;

		public float Fov { get { return fov; } set { fov = value; UpdatePerspective(); } }
		public float Aspect { get { return aspect; } set { aspect = value; UpdatePerspective(); } }

		public float NearZ { get { return nearZ; } set { nearZ = value; UpdatePerspective(); } }
		public float FarZ { get { return farZ; } set { farZ = value; UpdatePerspective(); } }

		public Vector3 Position { get { return position; } set { if (position != value) { position = value; UpdateView(true, true); } } }
		public Vector3 Target { get { return target; } set { if (target != value) { target = value; UpdateView(false, true); } } }
		public Vector3 Direction { get { return Vector3.Normalize(Target - Position); } }

		public Matrix View			{ get; private set; }
		public Matrix Projection	{ get; private set; }

		public event CameraChangedEventHandler CameraChanged;

		public Vector3 YawPitchRoll
		{
			get
			{
				Vector2 plane = new Vector2(Direction.X, Direction.Z);
				float yaw = (float)Math.Atan2(plane.X, plane.Y);
				float pitch = (float)(Math.Atan2(Direction.Y, plane.Length()));
				return new Vector3(yaw, pitch, 0);
			}
		}


		public Camera()
		{ }

		protected virtual void FireCameraChangedEvent(bool posChanged, bool viewChanged, bool projChanged)
		{
			if (CameraChanged == null) return;
			CameraChangedEventArgs e = new CameraChangedEventArgs(posChanged,viewChanged, projChanged, this);
			CameraChanged(this, e);
		}

		public void LookAt( Vector3 pos, Vector3 target )
		{
			freezeUpdates = true;
			bool posChanged = (pos != Position);
			Position = pos;
			bool dirChanged = posChanged || (target != Target);
			Target = target;
			freezeUpdates = false;
			UpdateView(posChanged, dirChanged);
		}

		public void UpdateView(bool posChanged, bool dirChanged)
		{
			if (freezeUpdates) return;
			Matrix prevView = View;
			View = Matrix.LookAtLH(Position, Target, Vector3.UnitY);
			if (!View.Equals(prevView))
				FireCameraChangedEvent(posChanged, dirChanged, false);
		}

		public void UpdatePerspective( float fov, float aspect, float near, float far )
		{
			freezeUpdates = true;
			Fov    = fov;
			Aspect = aspect;
			NearZ   = near;
			FarZ = far;
			freezeUpdates = false;
			UpdatePerspective();
		}

		public void UpdatePerspective()
		{
			if (freezeUpdates) return;
			Matrix prevProj = Projection;
			Projection = Matrix.PerspectiveFovLH(Fov, Aspect, NearZ, FarZ);
			if (!prevProj.Equals(Projection))
				FireCameraChangedEvent(false,false,true);
		}
	}
}
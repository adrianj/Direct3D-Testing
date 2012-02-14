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

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class Camera
	{
		public float Fov			{ get; private set; }
		public float Aspect			{ get; private set; }

		public float Near			{ get; private set; }
		public float Far			{ get; private set; }

		public Vector3 Position		{ get; private set; }
		public Vector3 Direction	{ get; private set; }

		public Matrix View			{ get; private set; }
		public Matrix Projection	{ get; private set; }

		public event EventHandler CameraChanged;

		public Camera()
		{ }

		protected virtual void FireCameraChangedEvent()
		{
			if (CameraChanged != null)
				CameraChanged(this, EventArgs.Empty);
		}

		public void LookAt( Vector3 pos, Vector3 target )
		{
			Position = pos;
			Direction = Vector3.Normalize( target - pos );
			Matrix prevView = View;
			View = Matrix.LookAtLH( pos, target, Vector3.UnitY );
			if (!View.Equals(prevView))
				FireCameraChangedEvent();
		}

		public void Persepective( float fov, float aspect, float near, float far )
		{
			Fov    = fov;
			Aspect = aspect;
			Near   = near;
			Far    = far;
			Matrix prevProj = Projection;
			Projection = Matrix.PerspectiveFovLH( fov, aspect, near, far );
			if (!prevProj.Equals(Projection))
				FireCameraChangedEvent();
		}
	}
}
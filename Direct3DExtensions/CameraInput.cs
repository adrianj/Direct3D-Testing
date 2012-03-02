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

	using System.Drawing;
	using System.Windows.Forms;
	using System.Diagnostics;
	using System.ComponentModel;

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface CameraInput
	{
		Camera Camera { get; set; }
		InputHelper Input { get; set; }
		float TimeDelta { get;  }
		float Speed { get; }
		void OnRender();
		void AttachToControl(Control control);
		void SetSize(int width, int height);
		void LookAt(Vector3 position,Vector3 target);
		event CameraChangedEventHandler CameraChanged;
	}

	public class FirstPersonCameraInput : CameraInput
	{
		const float DefaultSpeed	= 0.125f;
		const float MinSpeed		= 0.0375f;
		const float MaxSpeed		= 128.0f;

		const float Sensitivity		= 0.00004f;
		float baseSpeed;
		float fov { get { return Camera.Fov * 180.0f / (float)System.Math.PI; } }

		public float Speed { get; private set; }

		// Timing
		protected Stopwatch stopwatch;
		long frametime;
		long lasttime;

		Control control;
		Camera camera;
		InputHelper input = new InputHelper();

		public event CameraChangedEventHandler CameraChanged;

		public Camera Camera { get { return camera; } set { SetCamera(value); } }
		public float TimeDelta {get;private set;}

		private void SetCamera(Camera value)
		{
			if (camera != null)
			{
				camera.CameraChanged -= FireCamerChangedEvent;
			}
			camera = value;
			camera.CameraChanged += FireCamerChangedEvent;
		}
		protected void FireCamerChangedEvent(object s, CameraChangedEventArgs e) { FireCameraChangedEvent(e); }
		protected virtual void FireCameraChangedEvent(CameraChangedEventArgs e)
		{
			if (CameraChanged != null)
				CameraChanged(this, e);
		}

		public InputHelper Input { get { return input; } set { input = value; AttachToControl(control); } }

		

		public FirstPersonCameraInput(Control control)
		{
			Camera = new Camera();
			this.AttachToControl(control);
			this.SetSize(control.Width, control.Height);

			baseSpeed = DefaultSpeed;
			stopwatch = Stopwatch.StartNew();
		}

		public void AttachToControl(Control control)
		{
			this.control = control;
			Input.AttachToControl(control);
		}

		public void SetSize(int width, int height)
		{
			Camera.UpdatePerspective(45.0f * (float)Math.PI / 180.0f, (float)width / (float)height, Camera.NearZ, Camera.FarZ);
		}

		public virtual void LookAt(Vector3 position, Vector3 target)
		{
			Camera.LookAt(position, target);
		}

		public virtual void OnRender()
		{
			TimeDelta = UpdateTiming();

			Input.Update();

			Vector3 targetdir = UpdateDirection();
			Speed = UpdateSpeed(TimeDelta);

			Vector3 move = UpdateMovement(Speed);


			MoveAndDirectCamera(targetdir, move);


			if( Input.HasFocus )
				Input.ForcePosition();
		}

		protected virtual void MoveAndDirectCamera(Vector3 direction, Vector3 movement)
		{
			Vector3 pos = MoveRelativeToWorld(movement, Camera);
			this.LookAt(pos, pos + direction);
		}

		public static Vector3 MoveRelativeToCamera(Vector3 movement, Camera camera)
		{
			Vector3 right = new Vector3(camera.View.M11, camera.View.M21, camera.View.M31);
			Vector3 pos = camera.Position + right * movement.X + Vector3.UnitY * movement.Y + camera.Direction * movement.Z;
			return pos;
		}

		public static Vector3 MoveRelativeToWorld(Vector3 movement, Camera camera)
		{
			if (movement == Vector3.Zero) return camera.Position;
			double Pan = camera.YawPitchRoll.X;
			double z = Math.Cos(Pan) * movement.Z + Math.Sin(Pan) * -movement.X;
			double x = Math.Cos(Pan) * movement.X - Math.Sin(Pan) * -movement.Z;
			Vector3 pos = camera.Position + new Vector3((float)x, (float)movement.Y, (float)z);
			return pos;
		}

		protected virtual Vector3 UpdateMovement(float speed)
		{
			// Update Position
			Vector3 move = Vector3.Zero;

			if (Input.IsKeyDown(Keys.Up) || Input.IsKeyDown(Keys.W)) move.Z += 1.0f;
			if (Input.IsKeyDown(Keys.Down) || Input.IsKeyDown(Keys.S)) move.Z -= 1.0f;
			if (Input.IsKeyDown(Keys.Right) || Input.IsKeyDown(Keys.D)) move.X += 1.0f;
			if (Input.IsKeyDown(Keys.Left) || Input.IsKeyDown(Keys.A)) move.X -= 1.0f;
			if (Input.IsKeyDown(Keys.PageUp) || Input.IsKeyDown(Keys.E)) move.Y += 1.0f;
			if (Input.IsKeyDown(Keys.PageDown) || Input.IsKeyDown(Keys.Q)) move.Y -= 1.0f;

			move = Vector3.Normalize(move);
			move *= speed;
			return move;
		}

		protected virtual float UpdateSpeed(float timeDelta)
		{
			int mouseDelta = Input.GetMouseWheelDelta();
			if (mouseDelta != 0)
			{
				float speedDelta = 2;
				if (mouseDelta < 0)
					speedDelta = 1.0f/2.0f;
				baseSpeed = Clamp(baseSpeed * speedDelta, MinSpeed, MaxSpeed);
			}

			float modifiedSpeed = baseSpeed;
			if (Input.IsKeyDown(Keys.ShiftKey))
			{
				modifiedSpeed *= 4.0f;
			}
			modifiedSpeed *= timeDelta;
			return modifiedSpeed;
		}

		protected virtual Vector3 UpdateDirection()
		{
			Vector3 dir = camera.Direction;
			if (Input.HasFocus)
			{
				// Update Rotation
				Point pt = Input.RelativeMousePosition;

				if (pt != Point.Empty)
				{
					Vector3 ypr = camera.YawPitchRoll;
					Vector3 rotate = new Vector3(pt.X, -pt.Y, 0) * Sensitivity * fov;
					rotate += ypr;
					rotate.Y = Clamp(-rotate.Y, -1.55f, 1.55f);
					dir = Vector3.TransformCoordinate(Vector3.UnitZ, Matrix.RotationYawPitchRoll(rotate.X,rotate.Y,0));
				}
			}
			return dir;
		}

		private float UpdateTiming()
		{
			long time = stopwatch.ElapsedMilliseconds;
			frametime = time - lasttime;
			lasttime = time;
			float dt = (float)frametime * 0.01f;
			return dt;
		}

		public override string ToString()
		{
			return "Pos: " + Camera.Position + ", Dir: " + Camera.Direction;
		}

		static float Clamp( float value, float min, float max )
		{
			if( value < min ) value = min;
			else if( value > max ) value = max;
			return value;
		}
	}
}
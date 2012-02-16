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
		void OnRender();
		void AttachToControl(Control control);
		void SetSize(int width, int height);
		void LookAt(Vector3 position,Vector3 target);
		event EventHandler CameraChanged;
	}

	public class FirstPersonCameraInput : CameraInput
	{
		const float DefaultSpeed	= 0.125f;
		const float MinSpeed		= 0.0375f;
		const float MaxSpeed		= 16.0f;

		const float Sensitivity		= 0.00004f;
		const float MinAngle		= -89.0f * (float)System.Math.PI / 180.0f;
		const float MaxAngle		=  89.0f * (float)System.Math.PI / 180.0f;
		float baseSpeed;
		Vector2 rotation;
		float fov { get { return Camera.Fov * 180.0f / (float)System.Math.PI; } }

		// Timing
		protected Stopwatch stopwatch;
		long frametime;
		long lasttime;

		Control control;
		Camera camera;
		InputHelper input = new InputHelper();

		public event EventHandler CameraChanged;

		public Camera Camera { get { return camera; } set { SetCamera(value); } }

		private void SetCamera(Camera value)
		{
			if (camera != null)
			{
				camera.CameraChanged -= FireCamerChangedEvent;
			}
			camera = value;
			camera.CameraChanged += FireCamerChangedEvent;
		}
		protected void FireCamerChangedEvent(object s, EventArgs e) { FireCameraChangedEvent(); }
		protected virtual void FireCameraChangedEvent()
		{
			if (CameraChanged != null)
				CameraChanged(this, EventArgs.Empty);
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
			Camera.Persepective(45.0f * (float)Math.PI / 180.0f, (float)width / (float)height, Camera.Near, Camera.Far);
		}

		public virtual void LookAt(Vector3 position, Vector3 target)
		{
			Camera.LookAt(position, target);
			rotation.X = -(float)Math.Atan2(Camera.Direction.Z, Camera.Direction.X) + (float)Math.PI * 0.5f;
			rotation.Y = (float)Math.Acos(Camera.Direction.Y) - (float)System.Math.PI * 0.5f;
		}

		public virtual void OnRender()
		{
			float dt = UpdateTiming();

			Input.Update();

			Vector3 targetdir = UpdateDirection();
			float speed = UpdateSpeed(dt);

			Vector3 move = UpdateMovement(speed);


			MoveAndDirectCamera(targetdir, move);


			if( Input.HasFocus )
				Input.ForcePosition();
		}

		protected virtual void MoveAndDirectCamera(Vector3 direction, Vector3 movement)
		{
			Vector3 pos = MoveRelativeToCamera(movement, Camera);
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
			Vector3 pos = camera.Position + movement;
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
			if (Input.HasFocus)
			{
				// Update Rotation
				Point pt = Input.RelativeMousePosition;

				if (pt != Point.Empty)
				{
					Vector2 rotate = new Vector2(pt.X, pt.Y) * Sensitivity * fov;
					rotation += rotate;
					rotation.Y = Clamp(rotation.Y, MinAngle, MaxAngle);
				}
			}

			Matrix rotmat = Matrix.RotationYawPitchRoll(rotation.X, rotation.Y, 0.0f);
			Vector3 targetdir = new Vector3(rotmat.M31, rotmat.M32, rotmat.M33);
			return targetdir;
		}

		private float UpdateTiming()
		{
			long time = stopwatch.ElapsedMilliseconds;
			frametime = time - lasttime;
			lasttime = time;
			float dt = (float)frametime * 0.01f;
			return dt;
		}

		static float Clamp( float value, float min, float max )
		{
			if( value < min ) value = min;
			else if( value > max ) value = max;
			return value;
		}
	}
}
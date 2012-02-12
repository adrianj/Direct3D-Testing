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

	public interface CameraInput
	{
		Camera Camera { get; set; }
		InputHelper Input { get; set; }
		void OnRender();
		void SetSize(int width, int height);
	}

	public class FirstPersonCameraInput : CameraInput
	{
		const float DefaultSpeed	= 0.125f;
		const float MinSpeed		= 0.05f;
		const float MaxSpeed		= 6.0f;

		const float Sensitivity		= 0.00004f;
		const float MinAngle		= -89.0f * (float)System.Math.PI / 180.0f;
		const float MaxAngle		=  89.0f * (float)System.Math.PI / 180.0f;
	

		public Camera Camera { get; set; }
		public InputHelper Input { get; set; }

		float		speed;
		Vector2		rotation;
		float		fov;

		// Timing
		protected Stopwatch stopwatch;
		long frametime;
		long lasttime;

		public FirstPersonCameraInput( Camera camera, InputHelper input )
		{
			this.Camera = camera;
			this.Input = input;

			rotation.X = -(float)Math.Atan2(camera.Direction.Z, camera.Direction.X) + (float)Math.PI * 0.5f;
			rotation.Y = (float)Math.Acos(camera.Direction.Y) - (float)System.Math.PI * 0.5f;

			speed = DefaultSpeed;
			fov = camera.Fov * 180.0f / (float)System.Math.PI;
			stopwatch = Stopwatch.StartNew();
		}

		public void SetSize(int width, int height)
		{
			Camera.Persepective(45.0f * (float)Math.PI / 180.0f, width / (float)height, 0.025f, 1200.0f);
		}

		public void OnRender()
		{
			long time = stopwatch.ElapsedMilliseconds;
			frametime = time - lasttime;
			lasttime = time;

			Input.Update();



			float dt = frametime * 0.01f;
			if( Input.HasFocus )
			{			
				// Update Rotation
				Point pt = Input.RelativeMousePosition;

				if( pt != Point.Empty )
				{
					Vector2 rotate = new Vector2( pt.X, pt.Y ) * Sensitivity * fov;
					rotation += rotate;
					rotation.Y = Clamp( rotation.Y, MinAngle, MaxAngle );
				}
			}

			// Update Speed
			speed = Clamp( speed + Input.GetMouseWheelDelta() * speed * 0.003f, MinSpeed, MaxSpeed );	// This is works better for low speeds

			// Update Position
			Vector3 move = Vector3.Zero;

			if( Input.IsKeyDown( Keys.Up )	     || Input.IsKeyDown( Keys.W ) )	move.Z += 1.0f;
			if( Input.IsKeyDown( Keys.Down )     || Input.IsKeyDown( Keys.S ) )	move.Z -= 1.0f;
			if( Input.IsKeyDown( Keys.Right )    || Input.IsKeyDown( Keys.D ) )	move.X += 1.0f;
			if( Input.IsKeyDown( Keys.Left )     || Input.IsKeyDown( Keys.A ) )	move.X -= 1.0f;
			if( Input.IsKeyDown( Keys.PageUp )   || Input.IsKeyDown( Keys.E ) )	move.Y += 1.0f;
			if( Input.IsKeyDown( Keys.PageDown ) || Input.IsKeyDown( Keys.Q ) )	move.Y -= 1.0f;

			move = Vector3.Normalize( move );

			float tempspeed = speed;
			if( Input.IsKeyDown( Keys.ShiftKey ) )
				tempspeed *= 4.0f;

			move *= tempspeed * dt;

			// Update Rotation
			Matrix rotmat     = Matrix.RotationYawPitchRoll( rotation.X, rotation.Y, 0.0f );
			Vector3 targetdir = new Vector3( rotmat.M31, rotmat.M32, rotmat.M33 );

			// Update Position
			Matrix view = Camera.View;
			Vector3 right = new Vector3( view.M11, view.M21, view.M31 );
			Vector3 pos   = Camera.Position + right * move.X + Vector3.UnitY * move.Y + Camera.Direction * move.Z;

			Camera.Persepective( fov * (float)System.Math.PI / 180.0f, Camera.Aspect, Camera.Near, Camera.Far );
			Camera.LookAt( pos, pos + targetdir );


			if( Input.HasFocus )
				Input.ForcePosition();
		}

		static float Clamp( float value, float min, float max )
		{
			if( value < min ) value = min;
			else if( value > max ) value = max;
			return value;
		}
	}
}